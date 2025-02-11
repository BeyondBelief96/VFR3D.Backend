using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;
using VFR3D.Domain.Entities;
using VFR3D.Domain.ValueObjects.Airsigmets;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Interfaces;
using VFR3D.Infrastructure.Utilities;

namespace VFR3D.Infrastructure.Services.CronJobServices
{
    public class AirsigmetCronService : IAviationWeatherService<Airsigmet>
    {
        private readonly ILogger<AirsigmetCronService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly VFR3DDbContext _dbContext;
        private const string AirsigmetUrl = "https://aviationweather.gov/data/cache/airsigmets.cache.xml.gz";

        public AirsigmetCronService(
            ILogger<AirsigmetCronService> logger,
            IHttpClientFactory httpClientFactory,
            VFR3DDbContext dbContext)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _dbContext = dbContext;
        }

        public async Task PollWeatherDataAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Starting AIRSIGMET data fetch and storage");
                var xmlData = await FetchAirsigmetXmlDataAsync(cancellationToken);
                var airsigmetData = ParseAirsigmetXmlData(xmlData);
                await UpdateOrCreateAirsigmetsAsync(airsigmetData, cancellationToken);
                await PurgeExpiredAirsigmetsAsync(cancellationToken);
                _logger.LogInformation("Completed AIRSIGMET data update");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching AIRSIGMET data");
                throw;
            }
        }

        private async Task<string> FetchAirsigmetXmlDataAsync(CancellationToken cancellationToken)
        {
            using var client = _httpClientFactory.CreateClient();
            using var response = await client.GetStreamAsync(AirsigmetUrl, cancellationToken);
            using var decompressedStream = new System.IO.Compression.GZipStream(
                response,
                System.IO.Compression.CompressionMode.Decompress);
            using var reader = new StreamReader(decompressedStream);

            return await reader.ReadToEndAsync(cancellationToken);
        }

        private IEnumerable<Airsigmet> ParseAirsigmetXmlData(string xmlData)
        {
            var doc = XDocument.Parse(xmlData);
            var airsigmetElements = doc.Descendants("AIRSIGMET");

            return airsigmetElements.Select(element => new Airsigmet
            {
                RawText = element.Element("raw_text")?.Value,
                ValidTimeFrom = element.Element("valid_time_from")?.Value,
                ValidTimeTo = element.Element("valid_time_to")?.Value,
                MovementDirDegrees = ParsingUtilities.ParseNullableInt(element.Element("movement_dir_degrees")?.Value),
                MovementSpeedKt = ParsingUtilities.ParseNullableInt(element.Element("movement_speed_kt")?.Value),
                AirsigmetType = element.Element("airsigmet_type")?.Value,
                Altitude = ParseAltitude(element.Element("altitude")),
                Hazard = ParseHazard(element.Element("hazard")),
                Areas = ParseAreas(element.Elements("area"))
            }).ToList();
        }

        private static AirsigmetAltitude? ParseAltitude(XElement? element)
        {
            if (element == null) return null;

            return new AirsigmetAltitude
            {
                MinFtMsl = ParsingUtilities.ParseNullableInt(element.Attribute("min_ft_msl")?.Value),
                MaxFtMsl = ParsingUtilities.ParseNullableInt(element.Attribute("max_ft_msl")?.Value)
            };
        }

        private static AirsigmetHazard? ParseHazard(XElement? element)
        {
            if (element == null) return null;

            return new AirsigmetHazard
            {
                Type = element.Attribute("type")?.Value,
                Severity = element.Attribute("severity")?.Value
            };
        }

        private static List<AirsigmetArea>? ParseAreas(IEnumerable<XElement> elements)
        {
            var areas = elements.Select(areaElement =>
            {
                var points = areaElement.Elements("point")
                    .Select(pointElement => new AirsigmetPoint
                    {
                        Longitude = ParsingUtilities.ParseFloat(pointElement.Element("longitude")?.Value ?? "0"),
                        Latitude = ParsingUtilities.ParseFloat(pointElement.Element("latitude")?.Value ?? "0")
                    })
                    .ToList();

                return new AirsigmetArea
                {
                    NumPoints = ParsingUtilities.ParseInt(areaElement.Attribute("num_points")?.Value ?? "0"),
                    Points = points
                };
            }).ToList();

            return areas.Any() ? areas : null;
        }

        private async Task UpdateOrCreateAirsigmetsAsync(IEnumerable<Airsigmet> airsigmets, CancellationToken cancellationToken)
        {
            var airsigmetsList = airsigmets.ToList();
            var validTimeFroms = airsigmetsList
                .Where(a => a.ValidTimeFrom != null)
                .Select(a => a.ValidTimeFrom!)
                .ToList();

            // Get existing AIRSIGMETs by ValidTimeFrom
            var existingAirsigmets = await _dbContext.Airsigmets
                .Where(a => a.ValidTimeFrom != null && validTimeFroms.Contains(a.ValidTimeFrom))
                .ToDictionaryAsync(
                    a => a.ValidTimeFrom!,
                    a => a,
                    cancellationToken);

            foreach (var airsigmet in airsigmetsList)
            {
                if (airsigmet.ValidTimeFrom != null &&
                    existingAirsigmets.TryGetValue(airsigmet.ValidTimeFrom, out var existingAirsigmet))
                {
                    if (existingAirsigmet.RawText != airsigmet.RawText)
                    {
                        _logger.LogDebug("Updating existing AIRSIGMET for time {ValidTimeFrom}", airsigmet.ValidTimeFrom);
                        existingAirsigmet.RawText = airsigmet.RawText;
                        existingAirsigmet.ValidTimeTo = airsigmet.ValidTimeTo;
                        existingAirsigmet.MovementDirDegrees = airsigmet.MovementDirDegrees;
                        existingAirsigmet.MovementSpeedKt = airsigmet.MovementSpeedKt;
                        existingAirsigmet.AirsigmetType = airsigmet.AirsigmetType;
                        existingAirsigmet.Altitude = airsigmet.Altitude;
                        existingAirsigmet.Hazard = airsigmet.Hazard;
                        existingAirsigmet.Areas = airsigmet.Areas;
                    }
                }
                else
                {
                    _logger.LogDebug("Creating new AIRSIGMET for time {ValidTimeFrom}", airsigmet.ValidTimeFrom);
                    await _dbContext.Airsigmets.AddAsync(airsigmet, cancellationToken);
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private async Task PurgeExpiredAirsigmetsAsync(CancellationToken cancellationToken)
        {
            var currentTime = DateTime.UtcNow.ToString("O");

            var result = await _dbContext.Airsigmets
                .Where(a => a.ValidTimeTo != null && a.ValidTimeTo.CompareTo(currentTime) < 0)
                .ExecuteDeleteAsync(cancellationToken);

            _logger.LogInformation("Purged {Count} expired AIRSIGMETs", result);
        }
    }
}
