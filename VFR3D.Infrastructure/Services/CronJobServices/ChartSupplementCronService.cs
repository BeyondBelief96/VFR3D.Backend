using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO.Compression;
using System.Xml.Linq;
using VFR3D.Domain.Entities;
using VFR3D.Domain.ValueObjects.FaaPublications;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Interfaces;
using VFR3D.Infrastructure.Settings;
using VFR3D.Infrastructure.Utilities;

namespace VFR3D.Infrastructure.Services.CronJobServices
{
    public class ChartSupplementCronService : IChartSupplementCronService
    {
        private readonly ILogger<ChartSupplementCronService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly VFR3DDbContext _dbContext;
        private readonly ICloudStorageService _cloudStorageService;
        private readonly CloudStorageSettings _cloudStorageSettings;

        public ChartSupplementCronService(
            ILogger<ChartSupplementCronService> logger,
            IHttpClientFactory httpClientFactory,
            VFR3DDbContext dbContext,
            ICloudStorageService cloudStorageService,
            IOptions<CloudStorageSettings> cloudStorageSettings)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _dbContext = dbContext;
            _cloudStorageService = cloudStorageService;
            _cloudStorageSettings = cloudStorageSettings.Value;
        }

        public async Task DownloadAndProcessChartSupplementsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var currentDate = DateTime.UtcNow;

                var publicationCycle = await _dbContext.FaaPublicationCycles.FirstOrDefaultAsync(p => p.PublicationType == PublicationType.ChartSupplement);

                if (publicationCycle == null)
                {
                    throw new Exception("No publication cycle found for Chart Supplements.");
                }

                var currentPublicationDate = FaaPublicationDateUtils.CalculateCurrentPublicationDate(publicationCycle.KnownValidDate, publicationCycle.CycleLengthDays);
                var dateString = FaaPublicationDateUtils.FormatDateForChartSupplements(currentPublicationDate);
                var faaChartSupplementUrl = $"https://aeronav.faa.gov/Upload_313-d/supplements/DCS_{dateString}.zip";

                _logger.LogInformation(
                "Starting download from URL: {Url} for publication date: {PublicationDate}",
                faaChartSupplementUrl,
                currentPublicationDate);

                using var client = _httpClientFactory.CreateClient();
                using var response = await client.GetStreamAsync(faaChartSupplementUrl, cancellationToken);
                using var zipArchive = new ZipArchive(response);

                var xmlEntry = zipArchive.Entries.FirstOrDefault(e => e.Name.EndsWith(".xml"));
                if (xmlEntry == null)
                {
                    throw new Exception("Chart Supplment Database XML file not found in zip archive.");
                }

                using var xmlStream = xmlEntry.Open();
                using var reader = new StreamReader(xmlStream);
                var xmlContent = await reader.ReadToEndAsync();

                await ParseAndStoreXmlDataAsync(xmlContent, cancellationToken);

                var pdfEntries = zipArchive.Entries.Where(e => e.Name.EndsWith(".pdf"));
                await UploadPdfsToStorageAsync(pdfEntries, cancellationToken);

                _logger.LogInformation("Completed chart supplement processing.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chart supplements");
                throw;
            }
        }

        private async Task ParseAndStoreXmlDataAsync(string xmlContent, CancellationToken cancellationToken)
        {
            const int batchSize = 1000;

            try
            {
                // Parse XML and group data in memory first
                var doc = XDocument.Parse(xmlContent);
                var supplements = doc.Descendants("location")
                    .SelectMany(location => location.Elements("airport"))
                    .Select(airport => new ChartSupplement
                    {
                        AirportName = airport.Element("aptname")?.Value,
                        AirportCity = airport.Element("aptcity")?.Value,
                        NavigationalAidName = airport.Element("navidname")?.Value,
                        AirportCode = airport.Element("aptid")?.Value,
                        FileName = airport.Element("pages")?.Element("pdf")?.Value
                    })
                    .ToList();

                // Process airports
                var airportSupplements = supplements
                    .Where(s => !string.IsNullOrEmpty(s.AirportCode))
                    .ToList();

                if (airportSupplements.Any())
                {
                    await ProcessAirportBatch(airportSupplements, batchSize, cancellationToken);
                }

                // Process navaids
                var navaidSupplements = supplements
                    .Where(s => string.IsNullOrEmpty(s.AirportCode) && !string.IsNullOrEmpty(s.NavigationalAidName))
                    .ToList();

                if (navaidSupplements.Any())
                {
                    await ProcessNavaidBatch(navaidSupplements, batchSize, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chart supplements");
                throw;
            }
        }

        private async Task ProcessAirportBatch(List<ChartSupplement> supplements, int batchSize, CancellationToken cancellationToken)
        {
            for (int i = 0; i < supplements.Count; i += batchSize)
            {
                var batch = supplements.Skip(i).Take(batchSize).ToList();
                var batchCodes = batch.Select(s => s.AirportCode!).ToList();

                // Get existing records for this batch in one query
                var existingSupplements = await _dbContext.ChartSupplements
                    .Where(cs => cs.AirportCode != null && batchCodes.Contains(cs.AirportCode))
                    .ToListAsync(cancellationToken);

                // Group existing supplements by AirportCode only
                var existingLookup = existingSupplements
                    .ToLookup(x => x.AirportCode);

                foreach (var supplement in batch)
                {
                    // Look for an existing record with the same AirportCode
                    var existingMatch = existingLookup[supplement.AirportCode].FirstOrDefault();

                    if (existingMatch != null)
                    {
                        // Update existing record including FileName for new editions
                        await _dbContext.ChartSupplements
                            .Where(cs => cs.Id == existingMatch.Id)
                            .ExecuteUpdateAsync(s => s
                                .SetProperty(b => b.AirportName, supplement.AirportName)
                                .SetProperty(b => b.AirportCity, supplement.AirportCity)
                                .SetProperty(b => b.FileName, supplement.FileName),
                                cancellationToken);
                    }
                    else
                    {
                        // This is a new airport
                        await _dbContext.ChartSupplements.AddAsync(supplement, cancellationToken);
                    }
                }

                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Processed airport batch: {Count} supplements processed",
                    batch.Count);
            }
        }

        private async Task ProcessNavaidBatch(List<ChartSupplement> supplements, int batchSize, CancellationToken cancellationToken)
        {
            for (int i = 0; i < supplements.Count; i += batchSize)
            {
                var batch = supplements.Skip(i).Take(batchSize).ToList();
                var batchNames = batch.Select(s => s.NavigationalAidName!).ToList();

                // Get existing records for this batch in one query
                var existingSupplements = await _dbContext.ChartSupplements
                    .Where(cs => cs.NavigationalAidName != null && batchNames.Contains(cs.NavigationalAidName))
                    .ToListAsync(cancellationToken);

                // Group existing supplements by NavigationalAidName only
                var existingLookup = existingSupplements
                    .ToLookup(x => x.NavigationalAidName);

                foreach (var supplement in batch)
                {
                    // Look for an existing record with the same NavigationalAidName
                    var existingMatch = existingLookup[supplement.NavigationalAidName].FirstOrDefault();

                    if (existingMatch != null)
                    {
                        // Update existing record including FileName for new editions
                        await _dbContext.ChartSupplements
                            .Where(cs => cs.Id == existingMatch.Id)
                            .ExecuteUpdateAsync(s => s
                                .SetProperty(b => b.AirportName, supplement.AirportName)
                                .SetProperty(b => b.AirportCity, supplement.AirportCity)
                                .SetProperty(b => b.FileName, supplement.FileName),
                                cancellationToken);
                    }
                    else
                    {
                        // This is a new navaid
                        await _dbContext.ChartSupplements.AddAsync(supplement, cancellationToken);
                    }
                }

                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Processed navaid batch: {Count} supplements processed",
                    batch.Count);
            }
        }

        private async Task UploadPdfsToStorageAsync(IEnumerable<ZipArchiveEntry> pdfEntries, CancellationToken cancellationToken)
        {
            var containerName = _cloudStorageSettings.ChartSupplementsContainerName;
            var existingObjects = new Dictionary<string, string>();

            try
            {
                _logger.LogInformation("Listing existing blobs in container: {ContainerName}", containerName);
                var existingBlobs = await _cloudStorageService.ListBlobsAsync(containerName);

                foreach (var blobName in existingBlobs)
                {
                    var baseName = ExtractBaseName(blobName);
                    existingObjects[baseName] = blobName;
                }

                _logger.LogInformation("Found {Count} existing chart supplements in storage", existingObjects.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing existing blobs in container: {ContainerName}", containerName);
                throw;
            }

            foreach (var pdfEntry in pdfEntries)
            {
                try
                {
                    var baseName = ExtractBaseName(pdfEntry.Name);

                    if (existingObjects.TryGetValue(baseName, out var existingKey))
                    {
                        _logger.LogInformation(
                            "Found existing chart supplement {ExistingKey}, updating to new edition {NewKey}",
                            existingKey,
                            pdfEntry.Name);

                        await _cloudStorageService.DeleteBlobAsync(containerName, existingKey);
                        _logger.LogDebug("Deleted old edition {OldKey}", existingKey);
                    }

                    using var pdfStream = pdfEntry.Open();
                    using var memoryStream = new MemoryStream();
                    await pdfStream.CopyToAsync(memoryStream, cancellationToken);
                    memoryStream.Position = 0;

                    await _cloudStorageService.UploadBlobAsync(containerName, pdfEntry.Name, memoryStream, "application/pdf");
                    _logger.LogDebug("Uploaded {Action} PDF {FileName} to storage",
                        existingObjects.ContainsKey(baseName) ? "updated" : "new",
                        pdfEntry.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing PDF {FileName}", pdfEntry.Name);
                    throw;
                }
            }
        }

        private static string ExtractBaseName(string chartSupplementFileName)
        {
            // Assuming format is always like "NW_39_26DEC2024.pdf"
            var parts = chartSupplementFileName.Split('_');
            if (parts.Length >= 2)
            {
                return $"{parts[0]}_{parts[1]}";  // Returns "NW_39"
            }

            throw new ArgumentException($"Invalid file name format: {chartSupplementFileName}");
        }
    }
}
