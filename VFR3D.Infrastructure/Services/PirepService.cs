using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;
using VFR3D.Domain.Entities;
using VFR3D.Domain.ValueObjects.Pireps;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Services.Interfaces;
using VFR3D.Infrastructure.Utilities;

namespace VFR3D.Infrastructure.Services
{
    public class PirepService : IAviationWeatherService<Pirep>
    {
        private readonly ILogger<PirepService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly CronServiceDbContext _dbContext;
        private const string PirepUrl = "https://aviationweather.gov/data/cache/aircraftreports.cache.xml.gz";

        public PirepService(
            ILogger<PirepService> logger,
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
                _logger.LogInformation("Starting PIREP data fetch and storage");
                var xmlData = await FetchPirepXmlDataAsync(cancellationToken);
                var pirepData = ParsePirepXmlData(xmlData);
                await UpdateOrCreatePirepsAsync(pirepData, cancellationToken);
                await PurgeExpiredPirepsAsync(cancellationToken);
                _logger.LogInformation("Completed PIREP data update");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching PIREP data");
                throw;
            }
        }

        private async Task<string> FetchPirepXmlDataAsync(CancellationToken cancellationToken)
        {
            using var client = _httpClientFactory.CreateClient();
            using var response = await client.GetStreamAsync(PirepUrl, cancellationToken);
            using var decompressedStream = new System.IO.Compression.GZipStream(
                response,
                System.IO.Compression.CompressionMode.Decompress);
            using var reader = new StreamReader(decompressedStream);

            return await reader.ReadToEndAsync(cancellationToken);
        }

        private IEnumerable<Pirep> ParsePirepXmlData(string xmlData)
        {
            var pireps = new List<Pirep>();
            var doc = XDocument.Parse(xmlData);
            var pirepElements = doc.Descendants("AircraftReport");

            foreach (var element in pirepElements)
            {
                var pirep = new Pirep
                {
                    ReceiptTime = element.Element("receipt_time")?.Value,
                    ObservationTime = element.Element("observation_time")?.Value,
                    QualityControlFlags = ParseQualityControlFlags(element.Element("quality_control_flags")),
                    AircraftRef = element.Element("aircraft_ref")?.Value,
                    Latitude = ParsingUtilities.ParseNullableFloat(element.Element("latitude")?.Value),
                    Longitude = ParsingUtilities.ParseNullableFloat(element.Element("longitude")?.Value),
                    AltitudeFtMsl = ParsingUtilities.ParseNullableInt(element.Element("altitude_ft_msl")?.Value),
                    SkyConditions = ParseSkyConditions(element.Elements("sky_condition")),
                    TurbulenceConditions = ParseTurbulenceConditions(element.Elements("turbulence_condition")),
                    IcingConditions = ParseIcingConditions(element.Elements("icing_condition")),
                    VisibilityStatuteMi = ParsingUtilities.ParseNullableInt(element.Element("visibility_statute_mi")?.Value),
                    WxString = element.Element("wx_string")?.Value,
                    TempC = ParsingUtilities.ParseNullableFloat(element.Element("temp_c")?.Value),
                    WindDirDegrees = ParsingUtilities.ParseNullableInt(element.Element("wind_dir_degrees")?.Value),
                    WindSpeedKt = ParsingUtilities.ParseNullableInt(element.Element("wind_speed_kt")?.Value),
                    VertGustKt = ParsingUtilities.ParseNullableInt(element.Element("vert_gust_kt")?.Value),
                    ReportType = element.Element("report_type")?.Value,
                    RawText = element.Element("raw_text")?.Value
                };

                pireps.Add(pirep);
            }

            return pireps;
        }

        private static PirepQualityControlFlags? ParseQualityControlFlags(XElement? element)
        {
            if (element == null) return null;

            return new PirepQualityControlFlags
            {
                MidPointAssumed = element.Element("mid_point_assumed")?.Value,
                NoTimeStamp = element.Element("no_time_stamp")?.Value,
                FltLvlRange = element.Element("flt_lvl_range")?.Value,
                AboveGroundLevelIndicated = element.Element("above_ground_level_indicated")?.Value,
                NoFltLvl = element.Element("no_flt_lvl")?.Value,
                BadLocation = element.Element("bad_location")?.Value
            };
        }

        private static List<PirepSkyCondition>? ParseSkyConditions(IEnumerable<XElement> elements)
        {
            var conditions = elements.Select(element => new PirepSkyCondition
            {
                SkyCover = element.Attribute("sky_cover")?.Value ?? string.Empty,
                CloudBaseFtMsl = ParsingUtilities.ParseNullableInt(element.Attribute("cloud_base_ft_msl")?.Value),
                CloudTopFtMsl = ParsingUtilities.ParseNullableInt(element.Attribute("cloud_top_ft_msl")?.Value)
            }).ToList();

            return conditions.Any() ? conditions : null;
        }

        private static List<PirepTurbulenceCondition>? ParseTurbulenceConditions(IEnumerable<XElement> elements)
        {
            var conditions = elements.Select(element => new PirepTurbulenceCondition
            {
                TurbulenceType = element.Attribute("turbulence_type")?.Value,
                TurbulenceIntensity = element.Attribute("turbulence_intensity")?.Value,
                TurbulenceBaseFtMsl = ParsingUtilities.ParseNullableInt(element.Attribute("turbulence_base_ft_msl")?.Value),
                TurbulenceTopFtMsl = ParsingUtilities.ParseNullableInt(element.Attribute("turbulence_top_ft_msl")?.Value),
                TurbulenceFreq = element.Attribute("turbulence_freq")?.Value
            }).ToList();

            return conditions.Any() ? conditions : null;
        }

        private static List<PirepIcingCondition>? ParseIcingConditions(IEnumerable<XElement> elements)
        {
            var conditions = elements.Select(element => new PirepIcingCondition
            {
                IcingType = element.Attribute("icing_type")?.Value,
                IcingIntensity = element.Attribute("icing_intensity")?.Value,
                IcingBaseFtMsl = ParsingUtilities.ParseNullableInt(element.Attribute("icing_base_ft_msl")?.Value),
                IcingTopFtMsl = ParsingUtilities.ParseNullableInt(element.Attribute("icing_top_ft_msl")?.Value)
            }).ToList();

            return conditions.Any() ? conditions : null;
        }

        private async Task UpdateOrCreatePirepsAsync(IEnumerable<Pirep> pireps, CancellationToken cancellationToken)
        {
            var pirepsList = pireps.ToList();
            var now = DateTime.UtcNow;
            var thirtyMinutesAgo = now.AddMinutes(-30).ToString("O"); // ISO 8601 format

            // Filter out PIREPs older than 30 minutes and null observation times
            var validPireps = pirepsList
                .Where(p => p.ObservationTime != null &&
                            p.ObservationTime.CompareTo(thirtyMinutesAgo) >= 0)
                .ToList();

            // Get all existing PIREPs for these observations
            var observationTimes = validPireps
                .Where(p => p.ObservationTime != null)
                .Select(p => p.ObservationTime!)
                .ToList();

            var existingPireps = await _dbContext.Pireps
                .Where(p => p.ObservationTime != null && observationTimes.Contains(p.ObservationTime))
                .ToDictionaryAsync(
                    p => p.ObservationTime!, // The ! operator tells the compiler we know it's not null
                    p => p,
                    cancellationToken);

            foreach (var pirep in validPireps)
            {
                if (pirep.ObservationTime != null && existingPireps.TryGetValue(pirep.ObservationTime, out var existingPirep))
                {
                    if (existingPirep.RawText != pirep.RawText)
                    {
                        _logger.LogDebug("Updating existing PIREP for {aircraftRef}", pirep.AircraftRef);
                        existingPirep.ReceiptTime = pirep.ReceiptTime;
                        existingPirep.QualityControlFlags = pirep.QualityControlFlags;
                        existingPirep.AircraftRef = pirep.AircraftRef;
                        existingPirep.Latitude = pirep.Latitude;
                        existingPirep.Longitude = pirep.Longitude;
                        existingPirep.AltitudeFtMsl = pirep.AltitudeFtMsl;
                        existingPirep.SkyConditions = pirep.SkyConditions;
                        existingPirep.TurbulenceConditions = pirep.TurbulenceConditions;
                        existingPirep.IcingConditions = pirep.IcingConditions;
                        existingPirep.VisibilityStatuteMi = pirep.VisibilityStatuteMi;
                        existingPirep.WxString = pirep.WxString;
                        existingPirep.TempC = pirep.TempC;
                        existingPirep.WindDirDegrees = pirep.WindDirDegrees;
                        existingPirep.WindSpeedKt = pirep.WindSpeedKt;
                        existingPirep.VertGustKt = pirep.VertGustKt;
                        existingPirep.ReportType = pirep.ReportType;
                        existingPirep.RawText = pirep.RawText;
                    }
                }
                else
                {
                    _logger.LogDebug("Creating new PIREP for {aircraftRef}", pirep.AircraftRef);
                    await _dbContext.Pireps.AddAsync(pirep, cancellationToken);
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private async Task PurgeExpiredPirepsAsync(CancellationToken cancellationToken)
        {
            var thirtyMinutesAgo = DateTime.UtcNow.AddMinutes(-30).ToString("O"); // ISO 8601 format

            var result = await _dbContext.Pireps
                .Where(p => p.ObservationTime != null && p.ObservationTime.CompareTo(thirtyMinutesAgo) < 0)
                .ExecuteDeleteAsync(cancellationToken);

            _logger.LogInformation("Purged {Count} expired PIREPs", result);
        }
    }
}
