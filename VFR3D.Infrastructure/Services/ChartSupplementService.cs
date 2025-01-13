using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO.Compression;
using System.Xml.Linq;
using VFR3D.Domain.Entities;
using VFR3D.Domain.ValueObjects.FaaPublications;
using VFR3D.Infrastructure.Configuration;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Services.Interfaces;

namespace VFR3D.Infrastructure.Services
{
    public class ChartSupplementService : IChartSupplementService
    {
        private readonly ILogger<ChartSupplementService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly CronServiceDbContext _dbContext;
        private readonly IAmazonS3 _s3Client;
        private readonly AwsSettings _awsSettings;

        public ChartSupplementService(
        ILogger<ChartSupplementService> logger,
        IHttpClientFactory httpClientFactory,
        CronServiceDbContext dbContext,
        IAmazonS3 s3Client,
        IOptions<AwsSettings> awsSettings)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _dbContext = dbContext;
            _s3Client = s3Client;
            _awsSettings = awsSettings.Value;
        }

        public async Task DownloadAndProcessChartSupplementsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var currentDate = DateTime.UtcNow;

                var publicationCycle = await _dbContext.FaaPublicationCycles.FirstOrDefaultAsync(p => p.PublicationType == PublicationType.ChartSupplement);

                if(publicationCycle == null )
                {
                    throw new Exception("No publication cycle found for Chart Supplements.");
                }

                var daysSinceKnown = (currentDate - publicationCycle.KnownValidDate).TotalDays;
                var completeCycles = Math.Floor(daysSinceKnown / publicationCycle.CycleLengthDays);
                var currentPublicationDate = publicationCycle.KnownValidDate.AddDays(completeCycles * publicationCycle.CycleLengthDays);

                var dateString = FormatDate(currentPublicationDate);
                var faaChartSupplementUrl = $"https://aeronav.faa.gov/Upload_313-d/supplements/DCS_{dateString}.zip";

                _logger.LogInformation(
                "Starting download from URL: {Url} for publication date: {PublicationDate}",
                faaChartSupplementUrl,
                currentPublicationDate);

                using var client = _httpClientFactory.CreateClient();
                using var response = await client.GetStreamAsync(faaChartSupplementUrl, cancellationToken);
                using var zipArchive = new ZipArchive(response);

                var xmlEntry = zipArchive.Entries.FirstOrDefault(e => e.Name.EndsWith(".xml"));
                if(xmlEntry == null)
                {
                    throw new Exception("Chart Supplment Database XML file not found in zip archive.");
                }

                using var xmlStream = xmlEntry.Open();
                using var reader = new StreamReader(xmlStream);
                var xmlContent = await reader.ReadToEndAsync();

                await ParseAndStoreXmlDataAsync(xmlContent, cancellationToken);

                var pdfEntries = zipArchive.Entries.Where(e => e.Name.EndsWith(".pdf"));
                await UploadPdfsToS3Async(pdfEntries, cancellationToken);

                _logger.LogInformation("Completed chart supplement processing.");
            }
            catch(Exception ex)
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

                var existingDict = existingSupplements.ToDictionary(x => x.AirportCode!);

                // Update existing records
                foreach (var existing in existingSupplements)
                {
                    var newData = batch.First(s => s.AirportCode == existing.AirportCode);

                    await _dbContext.ChartSupplements
                        .Where(cs => cs.AirportCode == existing.AirportCode)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(b => b.AirportName, newData.AirportName)
                            .SetProperty(b => b.AirportCity, newData.AirportCity)
                            .SetProperty(b => b.FileName, newData.FileName),
                            cancellationToken);
                }

                // Insert new records
                var newRecords = batch
                    .Where(s => !existingDict.ContainsKey(s.AirportCode!))
                    .Select(s => new ChartSupplement
                    {
                        AirportCode = s.AirportCode,
                        NavigationalAidName = s.NavigationalAidName,
                        AirportName = s.AirportName,
                        AirportCity = s.AirportCity,
                        FileName = s.FileName
                    })
                    .ToList();

                if (newRecords.Any())
                {
                    await _dbContext.ChartSupplements.AddRangeAsync(newRecords, cancellationToken);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }

                _logger.LogInformation(
                    "Processed airport batch: {Updated} updated, {Inserted} inserted",
                    existingSupplements.Count,
                    newRecords.Count);
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

                var existingDict = existingSupplements.ToDictionary(x => x.NavigationalAidName!);

                foreach (var existing in existingSupplements)
                {
                    var newData = batch.First(s => s.NavigationalAidName == existing.NavigationalAidName);

                    await _dbContext.ChartSupplements
                        .Where(cs => cs.NavigationalAidName == existing.NavigationalAidName)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(b => b.AirportName, newData.AirportName)
                            .SetProperty(b => b.AirportCity, newData.AirportCity)
                            .SetProperty(b => b.FileName, newData.FileName),
                            cancellationToken);
                }

                var newRecords = batch
                    .Where(s => !existingDict.ContainsKey(s.NavigationalAidName!))
                    .Select(s => new ChartSupplement
                    {
                        AirportCode = s.AirportCode,
                        NavigationalAidName = s.NavigationalAidName,
                        AirportName = s.AirportName,
                        AirportCity = s.AirportCity,
                        FileName = s.FileName
                    })
                    .ToList();

                if (newRecords.Any())
                {
                    await _dbContext.ChartSupplements.AddRangeAsync(newRecords, cancellationToken);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }

                _logger.LogInformation(
                    "Processed navaid batch: {Updated} updated, {Inserted} inserted",
                    existingSupplements.Count,
                    newRecords.Count);
            }
        }

        private async Task UploadPdfsToS3Async(IEnumerable<ZipArchiveEntry> pdfEntries, CancellationToken cancellationToken)
        {
            var existingObjects = new Dictionary<string, string>();
            var listRequest = new ListObjectsV2Request
            {
                BucketName = _awsSettings.ChartSupplementsBucketName,
                MaxKeys = 1000
            };

            try
            {
                do
                {
                    var listResponse = await _s3Client.ListObjectsV2Async(listRequest, cancellationToken);

                    foreach(var item in listResponse.S3Objects)
                    {
                        var baseName = ExtractBaseName(item.Key);
                        existingObjects[baseName] = item.Key;
                    }

                    if(listResponse.IsTruncated)
                    {
                        listRequest.ContinuationToken = listResponse.NextContinuationToken;
                    }
                    else
                    {
                        break;
                    }
                }
                while (true);

                _logger.LogInformation("Found {Count} existing chart supplements in S3", existingObjects.Count);

            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Error listing existing objects in S3 Bucket: {_awsSettings.ChartSupplementsBucketName}");
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

                        var deleteRequest = new DeleteObjectRequest
                        {
                            BucketName = _awsSettings.ChartSupplementsBucketName,
                            Key = existingKey
                        };

                        await _s3Client.DeleteObjectAsync(deleteRequest, cancellationToken);
                        _logger.LogDebug("Deleted old edition {OldKey}", existingKey);
                    }

                    using var pdfStream = pdfEntry.Open();
                    using var memoryStream = new MemoryStream();
                    await pdfStream.CopyToAsync(memoryStream, cancellationToken);
                    memoryStream.Position = 0;

                    var putRequest = new PutObjectRequest
                    {
                        BucketName = _awsSettings.ChartSupplementsBucketName,
                        Key = pdfEntry.Name,
                        InputStream = memoryStream,
                        ContentType = "application/pdf"
                    };

                    await _s3Client.PutObjectAsync(putRequest, cancellationToken);
                    _logger.LogDebug("Uploaded {Action} PDF {FileName} to S3",
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

        private static string FormatDate(DateTime date)
        {
            return date.ToString("yyyyMMdd");
        }
    }
}
