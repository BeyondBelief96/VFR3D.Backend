using Amazon.S3.Model;
using Amazon.S3;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO.Compression;
using System.Xml.Linq;
using VFR3D.Domain.Entities;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using VFR3D.Domain.ValueObjects.FaaPublications;
using VFR3D.Infrastructure.Settings;

namespace VFR3D.Infrastructure.Services
{
    public class AirportDiagramService : IAirportDiagramService
    {
        private readonly ILogger<AirportDiagramService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly CronServiceDbContext _dbContext;
        private readonly IAmazonS3 _s3Client;
        private readonly AwsSettings _awsSettings;

        public AirportDiagramService(
            ILogger<AirportDiagramService> logger,
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

                var daysSinceKnown = (currentDate - publicationCycle.KnownValidDate).TotalDays;
                var completeCycles = Math.Floor(daysSinceKnown / publicationCycle.CycleLengthDays);
                var currentPublicationDate = publicationCycle.KnownValidDate.AddDays(completeCycles * publicationCycle.CycleLengthDays);

                var dateString = FormatDate(currentPublicationDate);
                var regions = new[] { "A", "B", "C", "D", "E" };

                await DeleteExistingS3Files(cancellationToken);

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

                    var pdfEntries = zipArchive.Entries.Where(e => e.Name.Contains("AD.PDF"));
                    await UploadPdfsToS3Async(pdfEntries, cancellationToken);
                }

                _logger.LogInformation("Completed airport diagram processing.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing airport diagrams");
                throw;
            }
        }

        private async Task DeleteExistingS3Files(CancellationToken cancellationToken)
        {
            try
            {
                var listRequest = new ListObjectsV2Request
                {
                    BucketName = _awsSettings.AirportDiagramsBucketName,
                    MaxKeys = 1000
                };

                var existingObjects = new List<KeyVersion>();

                do
                {
                    var listResponse = await _s3Client.ListObjectsV2Async(listRequest, cancellationToken);
                    existingObjects.AddRange(listResponse.S3Objects
                        .Select(obj => new KeyVersion { Key = obj.Key }));

                    if (listResponse.IsTruncated)
                    {
                        listRequest.ContinuationToken = listResponse.NextContinuationToken;
                    }
                    else
                    {
                        break;
                    }
                }
                while (true);

                _logger.LogInformation("Found {Count} existing airport diagrams in S3", existingObjects.Count);

                // Delete objects in batches of 1000
                for (int i = 0; i < existingObjects.Count; i += 1000)
                {
                    var batch = existingObjects.Skip(i).Take(1000).ToList();
                    var deleteRequest = new DeleteObjectsRequest
                    {
                        BucketName = _awsSettings.AirportDiagramsBucketName,
                        Objects = batch
                    };

                    _logger.LogInformation("Deleting batch of {Count} objects from S3", batch.Count);
                    await _s3Client.DeleteObjectsAsync(deleteRequest, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting existing S3 files: {BucketName}", _awsSettings.AirportDiagramsBucketName);
                throw;
            }
        }

        private async Task UploadPdfsToS3Async(IEnumerable<ZipArchiveEntry> pdfEntries, CancellationToken cancellationToken)
        {
            try
            {
                foreach (var pdfEntry in pdfEntries)
                {
                    using var pdfStream = pdfEntry.Open();
                    using var memoryStream = new MemoryStream();
                    await pdfStream.CopyToAsync(memoryStream, cancellationToken);
                    memoryStream.Position = 0;

                    var putRequest = new PutObjectRequest
                    {
                        BucketName = _awsSettings.AirportDiagramsBucketName,
                        Key = pdfEntry.Name,
                        InputStream = memoryStream,
                        ContentType = "application/pdf"
                    };

                    await _s3Client.PutObjectAsync(putRequest, cancellationToken);
                    _logger.LogDebug("Uploaded airport diagram {FileName} to S3", pdfEntry.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading PDFs to S3: {BucketName}", _awsSettings.AirportDiagramsBucketName);
                throw;
            }
        }

        private async Task ParseAndStoreXmlDataAsync(string xmlContent, CancellationToken cancellationToken)
        {
            var doc = XDocument.Parse(xmlContent);
            var diagrams = doc.Descendants("airport_name")
            .SelectMany(airport => airport.Elements("record")
            .Where(record => record.Element("chart_code")?.Value == "APD")
            .Select(record => new AirportDiagram
            {
                AirportName = airport.Attribute("ID")?.Value ?? "",
                IcaoIdent = airport.Attribute("icao_ident")?.Value,
                AirportIdent = airport.Attribute("apt_ident")?.Value,
                FileName = record.Element("pdf_name")?.Value
            }))
            .Where(diagram => diagram.AirportName != null &&
                   diagram.FileName != null)
            .ToList();

            foreach (var diagram in diagrams)
            {
                var existingDiagram = await _dbContext.AirportDiagrams
                    .FirstOrDefaultAsync(d =>
                        d.IcaoIdent == diagram.IcaoIdent ||
                        d.AirportIdent == diagram.AirportIdent,
                        cancellationToken);

                if (existingDiagram != null)
                {
                    existingDiagram.AirportName = diagram.AirportName;
                    existingDiagram.FileName = diagram.FileName;
                    _dbContext.AirportDiagrams.Update(existingDiagram);
                }
                else
                {
                    await _dbContext.AirportDiagrams.AddAsync(diagram, cancellationToken);
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private static string FormatDate(DateTime date)
        {
            return date.ToString("yyMMdd");
        }
    }
}
