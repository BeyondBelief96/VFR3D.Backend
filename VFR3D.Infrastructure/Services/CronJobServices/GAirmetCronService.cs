using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;
using VFR3D.Domain.Entities;
using VFR3D.Domain.ValueObjects.GAirmets;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Interfaces;
using VFR3D.Infrastructure.Utilities;

namespace VFR3D.Infrastructure.Services.CronJobServices
{
    public class GAirmetCronService : IAviationWeatherService<GAirmet>
    {
        private readonly ILogger<GAirmetCronService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly VFR3DDbContext _dbContext;
        private const string GAirmetUrl = "https://aviationweather.gov/data/cache/gairmets.cache.xml.gz";

        public GAirmetCronService(
            ILogger<GAirmetCronService> logger,
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
                _logger.LogInformation("Starting G-AIRMET data fetch and storage");

                // Purge expired records first to clean up old data
                await PurgeExpiredGAirmetsAsync(cancellationToken);

                var xmlData = await FetchGAirmetXmlDataAsync(cancellationToken);

                if (xmlData == null)
                {
                    _logger.LogInformation("No G-AIRMET data available from API (204 No Content)");
                    return;
                }

                var gairmetData = ParseGAirmetXmlData(xmlData);
                await UpdateOrCreateGAirmetsAsync(gairmetData, cancellationToken);
                _logger.LogInformation("Completed G-AIRMET data update");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching G-AIRMET data");
                throw;
            }
        }

        private async Task<string?> FetchGAirmetXmlDataAsync(CancellationToken cancellationToken)
        {
            using var client = _httpClientFactory.CreateClient();
            using var response = await client.GetAsync(GAirmetUrl, cancellationToken);

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    await using (var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken))
                    await using (var decompressedStream = new System.IO.Compression.GZipStream(
                        responseStream,
                        System.IO.Compression.CompressionMode.Decompress))
                    using (var reader = new StreamReader(decompressedStream))
                    {
                        return await reader.ReadToEndAsync(cancellationToken);
                    }

                case HttpStatusCode.NoContent:
                    return null;

                case HttpStatusCode.BadRequest:
                    _logger.LogError("Aviation Weather API returned 400 Bad Request for G-AIRMET data");
                    throw new HttpRequestException("Aviation Weather API returned 400 Bad Request - invalid parameters or URL");

                case HttpStatusCode.NotFound:
                    _logger.LogError("Aviation Weather API returned 404 Not Found for G-AIRMET endpoint");
                    throw new HttpRequestException("Aviation Weather API endpoint not found (404)");

                case HttpStatusCode.TooManyRequests:
                    _logger.LogWarning("Aviation Weather API rate limit exceeded (429 Too Many Requests)");
                    throw new HttpRequestException("Aviation Weather API rate limit exceeded (429)");

                case HttpStatusCode.InternalServerError:
                    _logger.LogError("Aviation Weather API returned 500 Internal Server Error");
                    throw new HttpRequestException("Aviation Weather API internal server error (500)");

                case HttpStatusCode.BadGateway:
                case HttpStatusCode.GatewayTimeout:
                    _logger.LogWarning("Aviation Weather API service disruption ({StatusCode})", (int)response.StatusCode);
                    throw new HttpRequestException($"Aviation Weather API service disruption ({(int)response.StatusCode})");

                default:
                    _logger.LogError("Aviation Weather API returned unexpected status code {StatusCode}", (int)response.StatusCode);
                    throw new HttpRequestException($"Aviation Weather API returned unexpected status code: {(int)response.StatusCode}");
            }
        }

        private IEnumerable<GAirmet> ParseGAirmetXmlData(string xmlData)
        {
            var doc = XDocument.Parse(xmlData);
            var gairmetElements = doc.Descendants("GAIRMET");

            return gairmetElements.Select(element =>
            {
                var hazardElement = element.Element("hazard");

                return new GAirmet
                {
                    ReceiptTime = ParseDateTime(element.Element("receipt_time")?.Value),
                    IssueTime = ParseDateTime(element.Element("issue_time")?.Value),
                    ExpireTime = ParseDateTime(element.Element("expire_time")?.Value),
                    ValidTime = ParseDateTime(element.Element("valid_time")?.Value),
                    Product = element.Element("product")?.Value ?? string.Empty,
                    Tag = element.Element("tag")?.Value,
                    ForecastHour = ParsingUtilities.ParseInt(
                        element.Element("roughly_the_number_of_hours_between_the_issue_time_and_the_valid_time")?.Value ?? "0"),
                    HazardType = hazardElement?.Attribute("type")?.Value,
                    HazardSeverity = hazardElement?.Attribute("severity")?.Value,
                    GeometryType = element.Element("geometry_type")?.Value,
                    DueTo = element.Element("due_to")?.Value,
                    Altitudes = ParseAltitudes(element),
                    Area = ParseArea(element.Element("area"))
                };
            }).ToList();
        }

        private static DateTime ParseDateTime(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return DateTime.MinValue;

            if (DateTime.TryParse(value, out var result))
                return result.ToUniversalTime();

            return DateTime.MinValue;
        }

        private static List<GAirmetAltitude>? ParseAltitudes(XElement gairmetElement)
        {
            var altitudeElements = gairmetElement.Elements("altitude").ToList();
            if (!altitudeElements.Any()) return null;

            var altitudes = new List<GAirmetAltitude>();

            foreach (var altElement in altitudeElements)
            {
                var altitude = new GAirmetAltitude
                {
                    MinFtMsl = altElement.Attribute("min_ft_msl")?.Value,
                    MaxFtMsl = altElement.Attribute("max_ft_msl")?.Value,
                    LevelFtMsl = altElement.Attribute("level_ft_msl")?.Value
                };

                // Check for fzl_altitude child element (follows the altitude element in the XML)
                var fzlElement = altElement.ElementsAfterSelf("fzl_altitude").FirstOrDefault();
                if (fzlElement != null)
                {
                    altitude.FzlAltitude = new GAirmetFzlAltitude
                    {
                        MinFtMsl = fzlElement.Element("min_ft_msl")?.Value,
                        MaxFtMsl = fzlElement.Element("max_ft_msl")?.Value
                    };
                }

                altitudes.Add(altitude);
            }

            return altitudes.Any() ? altitudes : null;
        }

        private static GAirmetArea? ParseArea(XElement? element)
        {
            if (element == null) return null;

            var points = element.Elements("point")
                .Select(pointElement => new GAirmetPoint
                {
                    Longitude = ParsingUtilities.ParseDouble(pointElement.Element("longitude")?.Value ?? "0"),
                    Latitude = ParsingUtilities.ParseDouble(pointElement.Element("latitude")?.Value ?? "0")
                })
                .ToList();

            if (!points.Any()) return null;

            return new GAirmetArea
            {
                NumPoints = ParsingUtilities.ParseInt(element.Attribute("num_points")?.Value ?? points.Count.ToString()),
                Points = points
            };
        }

        private async Task UpdateOrCreateGAirmetsAsync(IEnumerable<GAirmet> gairmets, CancellationToken cancellationToken)
        {
            var gairmetsList = gairmets.ToList();

            // Get existing G-AIRMETs and group by composite key (there can be multiple areas per key)
            var existingGAirmets = await _dbContext.GAirmets.ToListAsync(cancellationToken);

            // Group existing records by composite key to handle duplicates
            var existingByKey = existingGAirmets
                .GroupBy(g => CreateCompositeKey(g))
                .ToDictionary(g => g.Key, g => g.ToList());

            // Track which existing records we've matched (to detect records to remove)
            var matchedExistingIds = new HashSet<int>();

            foreach (var gairmet in gairmetsList)
            {
                var key = CreateCompositeKey(gairmet);

                if (existingByKey.TryGetValue(key, out var existingList))
                {
                    // Try to find an exact match by area
                    var exactMatch = existingList.FirstOrDefault(e =>
                        AreasEqual(e.Area, gairmet.Area) && !matchedExistingIds.Contains(e.Id));

                    if (exactMatch != null)
                    {
                        matchedExistingIds.Add(exactMatch.Id);

                        // Update if anything changed
                        if (exactMatch.DueTo != gairmet.DueTo ||
                            exactMatch.ExpireTime != gairmet.ExpireTime ||
                            exactMatch.HazardSeverity != gairmet.HazardSeverity)
                        {
                            _logger.LogDebug("Updating existing G-AIRMET for {Product} {Tag} at {ValidTime}",
                                gairmet.Product, gairmet.Tag, gairmet.ValidTime);
                            exactMatch.ReceiptTime = gairmet.ReceiptTime;
                            exactMatch.ExpireTime = gairmet.ExpireTime;
                            exactMatch.DueTo = gairmet.DueTo;
                            exactMatch.Altitudes = gairmet.Altitudes;
                            exactMatch.GeometryType = gairmet.GeometryType;
                            exactMatch.HazardSeverity = gairmet.HazardSeverity;
                        }
                    }
                    else
                    {
                        // No exact area match found, add as new
                        _logger.LogDebug("Creating new G-AIRMET for {Product} {Tag} at {ValidTime} (new area)",
                            gairmet.Product, gairmet.Tag, gairmet.ValidTime);
                        await _dbContext.GAirmets.AddAsync(gairmet, cancellationToken);
                    }
                }
                else
                {
                    // Completely new G-AIRMET
                    _logger.LogDebug("Creating new G-AIRMET for {Product} {Tag} at {ValidTime}",
                        gairmet.Product, gairmet.Tag, gairmet.ValidTime);
                    await _dbContext.GAirmets.AddAsync(gairmet, cancellationToken);
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private static string CreateCompositeKey(GAirmet gairmet)
        {
            return $"{gairmet.IssueTime:O}|{gairmet.ValidTime:O}|{gairmet.Product}|{gairmet.Tag}|{gairmet.HazardType}";
        }

        private static bool AreasEqual(GAirmetArea? area1, GAirmetArea? area2)
        {
            if (area1 == null && area2 == null) return true;
            if (area1 == null || area2 == null) return false;
            if (area1.NumPoints != area2.NumPoints) return false;
            if (area1.Points.Count != area2.Points.Count) return false;

            for (int i = 0; i < area1.Points.Count; i++)
            {
                if (Math.Abs(area1.Points[i].Latitude - area2.Points[i].Latitude) > 0.0001 ||
                    Math.Abs(area1.Points[i].Longitude - area2.Points[i].Longitude) > 0.0001)
                {
                    return false;
                }
            }

            return true;
        }

        private async Task PurgeExpiredGAirmetsAsync(CancellationToken cancellationToken)
        {
            var currentTime = DateTime.UtcNow;

            var result = await _dbContext.GAirmets
                .Where(g => g.ExpireTime < currentTime)
                .ExecuteDeleteAsync(cancellationToken);

            _logger.LogInformation("Purged {Count} expired G-AIRMETs", result);
        }
    }
}
