using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using VFR3D.Domain.Entities;
using VFR3D.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using VFR3D.Domain.ValueObjects.FaaPublications;
using VFR3D.Infrastructure.Settings;
using VFR3D.Infrastructure.Utilities;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.Infrastructure.Services.CronJobServices
{
    public class AirportDiagramCronService : IAirportDiagramCronService
    {
        private readonly ILogger<AirportDiagramCronService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly VFR3DDbContext _dbContext;
        private readonly ICloudStorageService _cloudStorageService;
        private readonly CloudStorageSettings _cloudStorageSettings;

        public AirportDiagramCronService(
            ILogger<AirportDiagramCronService> logger,
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

        public async Task DownloadAndProcessAirportDiagramsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var currentDate = DateTime.UtcNow;
                var publicationCycle = await _dbContext.FaaPublicationCycles.FirstOrDefaultAsync(
                    p => p.PublicationType == PublicationType.AirportDiagram);

                if (publicationCycle == null)
                {
                    throw new Exception("No publication cycle found for Airport Diagrams.");
                }

                var currentPublicationDate = FaaPublicationDateUtils.CalculateCurrentPublicationDate(publicationCycle.KnownValidDate, publicationCycle.CycleLengthDays);
                var dateString = FaaPublicationDateUtils.FormatDateForAirportDiagrams(currentPublicationDate);
                var regions = new[] { "A", "B", "C", "D", "E" };

                await DeleteExistingFilesAsync(cancellationToken);

                foreach (var region in regions)
                {
                    var faaUrl = $"https://aeronav.faa.gov/upload_313-d/terminal/DDTPP{region}_{dateString}.zip";

                    _logger.LogInformation(
                        "Starting download from URL: {Url} for region: {Region}",
                        faaUrl,
                        region);

                    using var client = _httpClientFactory.CreateClient();
                    using var response = await client.GetStreamAsync(faaUrl, cancellationToken);
                    using var zipArchive = new ZipArchive(response);

                    if (region == "E")
                    {
                        var xmlEntry = zipArchive.Entries.FirstOrDefault(e => e.Name.EndsWith(".xml"));
                        if (xmlEntry == null)
                        {
                            throw new Exception("Airport Diagram metadata XML file not found in zip archive.");
                        }

                        using var xmlStream = xmlEntry.Open();
                        using var reader = new StreamReader(xmlStream);
                        var xmlContent = await reader.ReadToEndAsync();

                        await ParseAndStoreXmlDataAsync(xmlContent, cancellationToken);
                    }

                    // Match airport diagram PDFs: 5-digit code + "AD" + optional variant + ".PDF"
                    // Examples: 00500AD.PDF, 00500ADROGERSLAKEBED.PDF, 00500ADROSAMONDLAKEBED.PDF
                    var airportDiagramPattern = new Regex(@"^\d{5}AD.*\.PDF$", RegexOptions.IgnoreCase);
                    var pdfEntries = zipArchive.Entries.Where(e => airportDiagramPattern.IsMatch(e.Name));
                    await UploadPdfsToStorageAsync(pdfEntries, cancellationToken);
                }

                _logger.LogInformation("Completed airport diagram processing.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing airport diagrams");
                throw;
            }
        }

        private async Task DeleteExistingFilesAsync(CancellationToken cancellationToken)
        {
            var containerName = _cloudStorageSettings.AirportDiagramsContainerName;

            try
            {
                var existingBlobs = await _cloudStorageService.ListBlobsAsync(containerName);

                _logger.LogInformation("Found {Count} existing airport diagrams in storage", existingBlobs.Count);

                if (existingBlobs.Count > 0)
                {
                    // DeleteBlobsAsync handles batching internally (256 per batch for Azure)
                    await _cloudStorageService.DeleteBlobsAsync(containerName, existingBlobs);
                    _logger.LogInformation("Deleted {Count} existing airport diagrams from storage", existingBlobs.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting existing files from container: {ContainerName}", containerName);
                throw;
            }
        }

        private async Task UploadPdfsToStorageAsync(IEnumerable<ZipArchiveEntry> pdfEntries, CancellationToken cancellationToken)
        {
            var containerName = _cloudStorageSettings.AirportDiagramsContainerName;

            try
            {
                // Collect all PDFs into memory for batch upload
                var blobs = new List<(string BlobName, byte[] Content, string ContentType)>();

                foreach (var pdfEntry in pdfEntries)
                {
                    using var pdfStream = pdfEntry.Open();
                    using var memoryStream = new MemoryStream();
                    await pdfStream.CopyToAsync(memoryStream, cancellationToken);
                    blobs.Add((pdfEntry.Name, memoryStream.ToArray(), "application/pdf"));
                }

                if (blobs.Count > 0)
                {
                    _logger.LogInformation("Uploading {Count} airport diagrams to storage", blobs.Count);
                    await _cloudStorageService.UploadBlobsAsync(containerName, blobs);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading PDFs to container: {ContainerName}", containerName);
                throw;
            }
        }

        private async Task ParseAndStoreXmlDataAsync(string xmlContent, CancellationToken cancellationToken)
        {
            const int batchSize = 1000;

            var doc = XDocument.Parse(xmlContent);
            var diagrams = doc.Descendants("airport_name")
            .SelectMany(airport => airport.Elements("record")
            .Where(record => record.Element("chart_code")?.Value == "APD")
            .Select(record => new AirportDiagram
            {
                AirportName = airport.Attribute("ID")?.Value ?? "",
                IcaoIdent = airport.Attribute("icao_ident")?.Value,
                AirportIdent = airport.Attribute("apt_ident")?.Value,
                ChartName = record.Element("chart_name")?.Value,
                FileName = record.Element("pdf_name")?.Value ?? ""
            }))
            .Where(diagram => !string.IsNullOrEmpty(diagram.AirportName) &&
                   !string.IsNullOrEmpty(diagram.FileName))
            .ToList();

            _logger.LogInformation("Processing {Count} airport diagrams from XML", diagrams.Count);

            for (int i = 0; i < diagrams.Count; i += batchSize)
            {
                var batch = diagrams.Skip(i).Take(batchSize).ToList();

                // Get all filenames for this batch
                var fileNames = batch.Select(d => d.FileName).ToList();

                // Get existing records for this batch by filename (unique identifier)
                var existingDiagrams = await _dbContext.AirportDiagrams
                    .Where(d => fileNames.Contains(d.FileName))
                    .ToListAsync(cancellationToken);

                // Create lookup by filename
                var existingByFileName = existingDiagrams.ToDictionary(d => d.FileName, d => d);

                var newDiagrams = new List<AirportDiagram>();

                foreach (var diagram in batch)
                {
                    if (existingByFileName.TryGetValue(diagram.FileName, out var existingDiagram))
                    {
                        // Update existing record
                        existingDiagram.AirportName = diagram.AirportName;
                        existingDiagram.IcaoIdent = diagram.IcaoIdent;
                        existingDiagram.AirportIdent = diagram.AirportIdent;
                        existingDiagram.ChartName = diagram.ChartName;
                    }
                    else
                    {
                        newDiagrams.Add(diagram);
                    }
                }

                // Batch add new diagrams
                if (newDiagrams.Count > 0)
                {
                    await _dbContext.AirportDiagrams.AddRangeAsync(newDiagrams, cancellationToken);
                }

                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Processed airport diagram batch: {Count} diagrams ({NewCount} new, {UpdatedCount} updated)",
                    batch.Count, newDiagrams.Count, batch.Count - newDiagrams.Count);
            }
        }
    }
}
