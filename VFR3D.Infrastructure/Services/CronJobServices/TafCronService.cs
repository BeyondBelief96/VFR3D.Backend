using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;
using VFR3D.Domain.Entities;
using VFR3D.Domain.ValueObjects.Taf;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Interfaces;
using VFR3D.Infrastructure.Utilities;

namespace VFR3D.Infrastructure.Services.CronJobServices
{
    public class TafCronService : IAviationWeatherService<Taf>
    {
        private readonly ILogger<TafCronService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly CronServiceDbContext _dbContext;
        private const string TafUrl = "https://aviationweather.gov/data/cache/tafs.cache.xml.gz";

        public TafCronService(
        ILogger<TafCronService> logger,
        IHttpClientFactory httpClientFactory,
        CronServiceDbContext dbContext)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _dbContext = dbContext;
        }

        public async Task PollWeatherDataAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Starting TAF data fetch and storage");
                var xmlData = await FetchTafXmlDataAsync(cancellationToken);
                var tafData = ParseTafXmlData(xmlData, cancellationToken);

                foreach (var taf in tafData)
                {
                    await UpdateOrCreateTafAsync(taf, cancellationToken);
                }

                _logger.LogInformation("Completed TAF data update");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching TAF data");
                throw;
            }
        }

        private async Task<string> FetchTafXmlDataAsync(CancellationToken cancellationToken)
        {
            using var client = _httpClientFactory.CreateClient();
            using var response = await client.GetStreamAsync(TafUrl, cancellationToken);
            using var decompressedStream = new System.IO.Compression.GZipStream(
                response,
                System.IO.Compression.CompressionMode.Decompress);
            using var reader = new StreamReader(decompressedStream);

            return await reader.ReadToEndAsync(cancellationToken);
        }

        private IEnumerable<Taf> ParseTafXmlData(string xmlData, CancellationToken cancellationToken)
        {
            var tafs = new List<Taf>();
            var doc = XDocument.Parse(xmlData);
            var tafElements = doc.Descendants("TAF");

            foreach (var element in tafElements)
            {
                var stationId = element.Element("station_id")?.Value;

                // Only process US stations (starting with K or P)
                if (string.IsNullOrEmpty(stationId) ||
                    !stationId.StartsWith("K") && !stationId.StartsWith("P"))
                {
                    continue;
                }

                var taf = new Taf
                {
                    RawText = element.Element("raw_text")?.Value,
                    StationId = stationId,
                    IssueTime = element.Element("issue_time")?.Value,
                    BulletinTime = element.Element("bulletin_time")?.Value,
                    ValidTimeFrom = element.Element("valid_time_from")?.Value,
                    ValidTimeTo = element.Element("valid_time_to")?.Value,
                    Remarks = element.Element("remarks")?.Value,
                    Latitude = ParsingUtilities.ParseNullableFloat(element.Element("latitude")?.Value),
                    Longitude = ParsingUtilities.ParseNullableFloat(element.Element("longitude")?.Value),
                    ElevationM = ParsingUtilities.ParseNullableFloat(element.Element("elevation_m")?.Value),
                    Forecast = ParseForecasts(element.Elements("forecast"))
                };

                tafs.Add(taf);
            }

            return tafs;
        }

        private static List<TafForecast>? ParseForecasts(IEnumerable<XElement> elements)
        {
            var forecasts = new List<TafForecast>();

            foreach (var element in elements)
            {
                var forecast = new TafForecast
                {
                    FcstTimeFrom = element.Element("fcst_time_from")?.Value,
                    FcstTimeTo = element.Element("fcst_time_to")?.Value,
                    ChangeIndicator = element.Element("change_indicator")?.Value,
                    TimeBecoming = element.Element("time_becoming")?.Value,
                    Probability = ParsingUtilities.ParseNullableInt(element.Element("probability")?.Value),
                    WindDirDegrees = element.Element("wind_dir_degrees")?.Value,
                    WindSpeedKt = ParsingUtilities.ParseNullableInt(element.Element("wind_speed_kt")?.Value),
                    WindGustKt = ParsingUtilities.ParseNullableInt(element.Element("wind_gust_kt")?.Value),
                    WindShearHgtFtAgl = ParsingUtilities.ParseNullableShort(element.Element("wind_shear_hgt_ft_agl")?.Value),
                    WindShearDirDegrees = ParsingUtilities.ParseNullableShort(element.Element("wind_shear_dir_degrees")?.Value),
                    WindShearSpeedKt = ParsingUtilities.ParseNullableInt(element.Element("wind_shear_speed_kt")?.Value),
                    VisibilityStatuteMi = element.Element("visibility_statute_mi")?.Value,
                    AltimInHg = ParsingUtilities.ParseNullableFloat(element.Element("altim_in_hg")?.Value),
                    VertVisFt = ParsingUtilities.ParseNullableShort(element.Element("vert_vis_ft")?.Value),
                    WxString = element.Element("wx_string")?.Value,
                    NotDecoded = element.Element("not_decoded")?.Value,
                    SkyConditions = ParseSkyConditions(element.Elements("sky_condition")),
                    TurbulenceConditions = ParseTurbulenceConditions(element.Elements("turbulence_condition")),
                    IcingConditions = ParseIcingConditions(element.Elements("icing_condition")),
                    Temperature = ParseTemperatures(element.Elements("temperature"))
                };

                forecasts.Add(forecast);
            }

            return forecasts.Any() ? forecasts : null;
        }

        private static List<TafSkyCondition> ParseSkyConditions(IEnumerable<XElement> elements)
        {
            return elements.Select(element => new TafSkyCondition
            {
                SkyCover = element.Attribute("sky_cover")?.Value ?? string.Empty,
                CloudBaseFtAgl = ParsingUtilities.ParseNullableInt(element.Attribute("cloud_base_ft_agl")?.Value),
                CloudType = element.Attribute("cloud_type")?.Value
            }).ToList();
        }

        private static List<TafTurbulenceCondition> ParseTurbulenceConditions(IEnumerable<XElement> elements)
        {
            return elements.Select(element => new TafTurbulenceCondition
            {
                TurbulenceIntensity = element.Attribute("turbulence_intensity")?.Value,
                TurbulenceMinAltFtAgl = ParsingUtilities.ParseNullableInt(element.Attribute("turbulence_min_alt_ft_agl")?.Value),
                TurbulenceMaxAltFtAgl = ParsingUtilities.ParseNullableInt(element.Attribute("turbulence_max_alt_ft_agl")?.Value)
            }).ToList();
        }

        private static List<TafIcingCondition> ParseIcingConditions(IEnumerable<XElement> elements)
        {
            return elements.Select(element => new TafIcingCondition
            {
                IcingIntensity = element.Attribute("icing_intensity")?.Value,
                IcingMinAltFtAgl = ParsingUtilities.ParseNullableInt(element.Attribute("icing_min_alt_ft_agl")?.Value),
                IcingMaxAltFtAgl = ParsingUtilities.ParseNullableInt(element.Attribute("icing_max_alt_ft_agl")?.Value)
            }).ToList();
        }

        private static List<TafTemperature> ParseTemperatures(IEnumerable<XElement> elements)
        {
            return elements.Select(element => new TafTemperature
            {
                ValidTime = element.Element("valid_time")?.Value,
                SfcTempC = ParsingUtilities.ParseNullableFloat(element.Element("sfc_temp_c")?.Value),
                MaxTempC = element.Element("max_temp_c")?.Value,
                MinTempC = element.Element("min_temp_c")?.Value
            }).ToList();
        }

        private async Task UpdateOrCreateTafAsync(Taf taf, CancellationToken cancellationToken)
        {
            var existingTaf = await _dbContext.Tafs
                .FirstOrDefaultAsync(t => t.StationId == taf.StationId, cancellationToken);

            if (existingTaf != null)
            {
                if (existingTaf.RawText != taf.RawText)
                {
                    _logger.LogDebug("Updating existing TAF for station {StationId}", taf.StationId);
                    existingTaf.RawText = taf.RawText;
                    existingTaf.IssueTime = taf.IssueTime;
                    existingTaf.BulletinTime = taf.BulletinTime;
                    existingTaf.ValidTimeFrom = taf.ValidTimeFrom;
                    existingTaf.ValidTimeTo = taf.ValidTimeTo;
                    existingTaf.Remarks = taf.Remarks;
                    existingTaf.Latitude = taf.Latitude;
                    existingTaf.Longitude = taf.Longitude;
                    existingTaf.ElevationM = taf.ElevationM;
                    existingTaf.Forecast = taf.Forecast;

                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    _logger.LogDebug("No changes for TAF station {StationId}", taf.StationId);
                }
            }
            else
            {
                _logger.LogDebug("Creating new TAF for station {StationId}", taf.StationId);
                await _dbContext.Tafs.AddAsync(taf, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
