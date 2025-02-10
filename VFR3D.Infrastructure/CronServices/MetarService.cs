using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;
using Vfr3d.Domain.Entities;
using VFR3D.Domain.ValueObjects.Metar;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Services.Interfaces;
using VFR3D.Infrastructure.Utilities;

namespace VFR3D.Infrastructure.Services
{
    public class MetarService : IAviationWeatherService<Metar>
    {
        private readonly ILogger<MetarService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly CronServiceDbContext _dbContext;
        private const string MetarUrl = "https://aviationweather.gov/data/cache/metars.cache.xml.gz";

        public MetarService(
        ILogger<MetarService> logger,
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
                _logger.LogInformation("Starting METAR data fetch and storage");
                var xmlData = await FetchMetarXmlDataAsync(cancellationToken);
                var metarData = ParseMetarXmlData(xmlData, cancellationToken);

                foreach (var metar in metarData)
                {
                    await UpdateOrCreateMetarAsync(metar, cancellationToken);
                }

                _logger.LogInformation("Completed METAR data update");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching METAR data");
                throw;
            }
        }

        private async Task<string> FetchMetarXmlDataAsync(CancellationToken cancellationToken)
        {
            using var client = _httpClientFactory.CreateClient();
            using var response = await client.GetStreamAsync(MetarUrl, cancellationToken);
            using var decompressedStream = new System.IO.Compression.GZipStream(
                response,
                System.IO.Compression.CompressionMode.Decompress);
            using var reader = new StreamReader(decompressedStream);

            return await reader.ReadToEndAsync(cancellationToken);
        }

        private IEnumerable<Metar> ParseMetarXmlData(string xmlData, CancellationToken cancellationToken)
        {
            var metars = new List<Metar>();
            var doc = XDocument.Parse(xmlData);
            var metarElements = doc.Descendants("METAR");

            foreach (var element in metarElements)
            {
                var stationId = element.Element("station_id")?.Value;

                // Only process US stations (starting with K or P)
                if (string.IsNullOrEmpty(stationId) ||
                    (!stationId.StartsWith("K") && !stationId.StartsWith("P")))
                {
                    continue;
                }

                var metar = new Metar
                {
                    RawText = element.Element("raw_text")?.Value,
                    StationId = stationId,
                    ObservationTime = element.Element("observation_time")?.Value,
                    Latitude = ParsingUtilities.ParseNullableFloat(element.Element("latitude")?.Value),
                    Longitude = ParsingUtilities.ParseNullableFloat(element.Element("longitude")?.Value),
                    TempC = ParsingUtilities.ParseNullableFloat(element.Element("temp_c")?.Value),
                    DewpointC = ParsingUtilities.ParseNullableFloat(element.Element("dewpoint_c")?.Value),
                    WindDirDegrees = element.Element("wind_dir_degrees")?.Value,
                    WindSpeedKt = ParsingUtilities.ParseNullableInt(element.Element("wind_speed_kt")?.Value),
                    WindGustKt = ParsingUtilities.ParseNullableInt(element.Element("wind_gust_kt")?.Value),
                    VisibilityStatuteMi = element.Element("visibility_statute_mi")?.Value,
                    AltimInHg = ParsingUtilities.ParseNullableFloat(element.Element("altim_in_hg")?.Value),
                    SeaLevelPressureMb = ParsingUtilities.ParseNullableFloat(element.Element("sea_level_pressure_mb")?.Value),
                    QualityControlFlags = ParseQualityControlFlags(element.Element("quality_control_flags")),
                    WxString = element.Element("wx_string")?.Value,
                    SkyCondition = ParseSkyConditions(element.Elements("sky_condition")),
                    FlightCategory = element.Element("flight_category")?.Value,
                    MetarType = element.Element("metar_type")?.Value,
                    ElevationM = ParsingUtilities.ParseNullableFloat(element.Element("elevation_m")?.Value)
                };

                metars.Add(metar);
            }

            return metars;
        }

        private async Task UpdateOrCreateMetarAsync(Metar metar, CancellationToken cancellationToken)
        {
            var existingMetar = await _dbContext.Metars
                .FirstOrDefaultAsync(m => m.StationId == metar.StationId, cancellationToken);

            if (existingMetar != null)
            {
                if (existingMetar.RawText != metar.RawText)
                {
                    _logger.LogDebug("Updating existing METAR for station {StationId}", metar.StationId);
                    existingMetar.RawText = metar.RawText;
                    existingMetar.ObservationTime = metar.ObservationTime;
                    existingMetar.Latitude = metar.Latitude;
                    existingMetar.Longitude = metar.Longitude;
                    existingMetar.TempC = metar.TempC;
                    existingMetar.DewpointC = metar.DewpointC;
                    existingMetar.WindDirDegrees = metar.WindDirDegrees;
                    existingMetar.WindSpeedKt = metar.WindSpeedKt;
                    existingMetar.WindGustKt = metar.WindGustKt;
                    existingMetar.VisibilityStatuteMi = metar.VisibilityStatuteMi;
                    existingMetar.AltimInHg = metar.AltimInHg;
                    existingMetar.SeaLevelPressureMb = metar.SeaLevelPressureMb;
                    existingMetar.QualityControlFlags = metar.QualityControlFlags;
                    existingMetar.WxString = metar.WxString;
                    existingMetar.SkyCondition = metar.SkyCondition;
                    existingMetar.FlightCategory = metar.FlightCategory;
                    existingMetar.ThreeHrPressureTendencyMb = metar.ThreeHrPressureTendencyMb;
                    existingMetar.MaxTC = metar.MaxTC;
                    existingMetar.MinTC = metar.MinTC;
                    existingMetar.MaxT24hrC = metar.MaxT24hrC;
                    existingMetar.MinT24hrC = metar.MinT24hrC;
                    existingMetar.PrecipIn = metar.PrecipIn;
                    existingMetar.Pcp3hrIn = metar.Pcp3hrIn;
                    existingMetar.Pcp6hrIn = metar.Pcp6hrIn;
                    existingMetar.Pcp24hrIn = metar.Pcp24hrIn;
                    existingMetar.SnowIn = metar.SnowIn;
                    existingMetar.VertVisFt = metar.VertVisFt;
                    existingMetar.MetarType = metar.MetarType;
                    existingMetar.ElevationM = metar.ElevationM;

                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    _logger.LogDebug("No changes for METAR station {StationId}", metar.StationId);
                }
            }
            else
            {
                _logger.LogDebug("Creating new METAR for station {StationId}", metar.StationId);
                await _dbContext.Metars.AddAsync(metar, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        private static MetarQualityControlFlags? ParseQualityControlFlags(XElement? element)
        {
            if (element == null) return null;

            return new MetarQualityControlFlags
            {
                Corrected = element.Element("corrected")?.Value,
                Auto = element.Element("auto")?.Value,
                AutoStation = element.Element("auto_station")?.Value,
                MaintenanceIndicatorOn = element.Element("maintenance_indicator_on")?.Value,
                NoSignal = element.Element("no_signal")?.Value,
                LightningSensorOff = element.Element("lightning_sensor_off")?.Value,
                FreezingRainSensorOff = element.Element("freezing_rain_sensor_off")?.Value,
                PresentWeatherSensorOff = element.Element("present_weather_sensor_off")?.Value
            };
        }

        private static List<MetarSkyCondition>? ParseSkyConditions(IEnumerable<XElement> elements)
        {
            var conditions = elements
                .Select(element => new MetarSkyCondition
                {
                    SkyCover = element.Attribute("sky_cover")?.Value ?? string.Empty,
                    CloudBaseFtAgl = ParsingUtilities.ParseNullableInt(element.Attribute("cloud_base_ft_agl")?.Value)
                })
                .ToList();

            return conditions.Any() ? conditions : null;
        }
    }
}
