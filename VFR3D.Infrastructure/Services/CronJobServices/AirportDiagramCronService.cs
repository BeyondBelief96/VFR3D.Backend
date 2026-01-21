using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO.Compression;
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

                    var pdfEntries = zipArchive.Entries.Where(e => e.Name.Contains("AD.PDF"));
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
                foreach (var pdfEntry in pdfEntries)
                {
                    using var pdfStream = pdfEntry.Open();
                    using var memoryStream = new MemoryStream();
                    await pdfStream.CopyToAsync(memoryStream, cancellationToken);
                    memoryStream.Position = 0;

                    await _cloudStorageService.UploadBlobAsync(containerName, pdfEntry.Name, memoryStream, "application/pdf");
                    _logger.LogDebug("Uploaded airport diagram {FileName} to storage", pdfEntry.Name);
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
    }
}
