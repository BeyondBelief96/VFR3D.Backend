using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using VFR3D.Infrastructure.Dtos.Navlog;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.Infrastructure.Services.WeatherServices
{
    public class WindsAloftService : IWindsAloftService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<WindsAloftService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private static readonly int[] AltitudeLevels = { 30, 60, 90, 120, 180, 240, 300, 340, 390 };

        public WindsAloftService(
            IHttpClientFactory httpClientFactory,
            ILogger<WindsAloftService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<WindsAloftDto> FetchWindsAloftData(int fcstHours)
        {
            try
            {
                if (fcstHours != 6 && fcstHours != 12 && fcstHours != 24)
                {
                    throw new ArgumentException("Forecast hours must be 6, 12, or 24", nameof(fcstHours));
                }

                var httpClient = _httpClientFactory.CreateClient(nameof(WindsAloftService));
                const string baseUrl = "https://aviationweather.gov/api/data/windtemp";
                var formattedFcst = fcstHours.ToString("D2");

                var textResponse = await FetchWithStatusHandlingAsync(
                    httpClient,
                    $"{baseUrl}?region=us&level=low&fcst={formattedFcst}",
                    "winds aloft text data");

                if (textResponse == null)
                {
                    _logger.LogInformation("No winds aloft data available from API (204 No Content)");
                    return new WindsAloftDto
                    {
                        ValidTime = DateTime.UtcNow,
                        ForUseStartTime = DateTime.UtcNow,
                        ForUseEndTime = DateTime.UtcNow.AddHours(6),
                        WindTemp = new List<WindsAloftSiteDto>()
                    };
                }

                var validTime = ExtractValidTime(textResponse);
                var (forUseStartTime, forUseEndTime) = ExtractForUseTimes(textResponse);

                var tasks = AltitudeLevels.Select(level =>
                    FetchWithStatusHandlingAsync(
                        httpClient,
                        $"{baseUrl}?region=us&level={level}&fcst={formattedFcst}&format=json",
                        $"winds aloft level {level} data"));

                var responses = await Task.WhenAll(tasks);
                var airportDataMap = new Dictionary<string, WindsAloftSiteDto>();

                foreach (var response in responses)
                {
                    if (response == null) continue;

                    var levelData = JsonSerializer.Deserialize<WindsAloftLevelData>(response, _jsonOptions);
                    if (levelData?.Sites == null) continue;

                    foreach (var site in levelData.Sites)
                    {
                        var altitude = (int.Parse(levelData.Level) * 100).ToString();

                        if (!airportDataMap.TryGetValue(site.Id, out var airportData))
                        {
                            airportData = new WindsAloftSiteDto
                            {
                                Id = site.Id,
                                Lat = site.Lat,
                                Lon = site.Lon,
                                WindTemp = new Dictionary<string, WindTempDto>()
                            };
                            airportDataMap[site.Id] = airportData;
                        }

                        airportData.WindTemp[altitude] = new WindTempDto
                        {
                            Direction = site.Dir == 990 ? null : site.Dir,
                            Speed = site.Spd,
                            Temperature = site.Temp
                        };
                    }
                }

                return new WindsAloftDto
                {
                    ValidTime = validTime,
                    ForUseStartTime = forUseStartTime,
                    ForUseEndTime = forUseEndTime,
                    WindTemp = airportDataMap.Values.ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching winds aloft data for forecast hours: {FcstHours}", fcstHours);
                throw;
            }
        }

        private async Task<string?> FetchWithStatusHandlingAsync(HttpClient httpClient, string url, string dataDescription)
        {
            using var response = await httpClient.GetAsync(url);

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    return await response.Content.ReadAsStringAsync();

                case HttpStatusCode.NoContent:
                    return null;

                case HttpStatusCode.BadRequest:
                    _logger.LogError("Aviation Weather API returned 400 Bad Request for {DataDescription}", dataDescription);
                    throw new HttpRequestException("Aviation Weather API returned 400 Bad Request - invalid parameters or URL");

                case HttpStatusCode.NotFound:
                    _logger.LogError("Aviation Weather API returned 404 Not Found for {DataDescription}", dataDescription);
                    throw new HttpRequestException("Aviation Weather API endpoint not found (404)");

                case HttpStatusCode.TooManyRequests:
                    _logger.LogWarning("Aviation Weather API rate limit exceeded (429 Too Many Requests) for {DataDescription}", dataDescription);
                    throw new HttpRequestException("Aviation Weather API rate limit exceeded (429)");

                case HttpStatusCode.InternalServerError:
                    _logger.LogError("Aviation Weather API returned 500 Internal Server Error for {DataDescription}", dataDescription);
                    throw new HttpRequestException("Aviation Weather API internal server error (500)");

                case HttpStatusCode.BadGateway:
                case HttpStatusCode.GatewayTimeout:
                    _logger.LogWarning("Aviation Weather API service disruption ({StatusCode}) for {DataDescription}", (int)response.StatusCode, dataDescription);
                    throw new HttpRequestException($"Aviation Weather API service disruption ({(int)response.StatusCode})");

                default:
                    _logger.LogError("Aviation Weather API returned unexpected status code {StatusCode} for {DataDescription}", (int)response.StatusCode, dataDescription);
                    throw new HttpRequestException($"Aviation Weather API returned unexpected status code: {(int)response.StatusCode}");
            }
        }

        private static DateTime ExtractValidTime(string rawText)
        {
            var validTimeMatch = System.Text.RegularExpressions.Regex.Match(rawText, @"VALID (\d{6})");
            if (!validTimeMatch.Success)
            {
                throw new FormatException("Valid time not found in raw text");
            }

            var validTimeString = validTimeMatch.Groups[1].Value;
            var day = int.Parse(validTimeString[..2]);
            var hour = int.Parse(validTimeString.Substring(2, 2));
            var minute = int.Parse(validTimeString.Substring(4, 2));

            var validTime = DateTime.UtcNow;
            return new DateTime(
                validTime.Year,
                validTime.Month,
                day,
                hour,
                minute,
                0,
                DateTimeKind.Utc);
        }

        private static (DateTime ForUseStartTime, DateTime ForUseEndTime) ExtractForUseTimes(string rawText)
        {
            var forUseMatch = System.Text.RegularExpressions.Regex.Match(rawText, @"FOR USE (\d{4})-(\d{4})");
            if (!forUseMatch.Success)
            {
                throw new FormatException("For use times not found in raw text");
            }

            var validTime = ExtractValidTime(rawText);
            
            var startHour = int.Parse(forUseMatch.Groups[1].Value[..2]);
            var startMinute = int.Parse(forUseMatch.Groups[1].Value.Substring(2, 2));
            
            var forUseStartTime = new DateTime(
                validTime.Year,
                validTime.Month,
                validTime.Day,
                startHour,
                startMinute,
                0,
                DateTimeKind.Utc);
            // If the "for use" start time is earlier than the valid time, it refers to the same day as the valid time
            // If the "for use" start time is later than the valid time, it refers to the day before the valid time
            if (startHour > validTime.Hour)
            {
                forUseStartTime = forUseStartTime.AddDays(-1);
            }

            var endHour = int.Parse(forUseMatch.Groups[2].Value[..2]);
            var endMinute = int.Parse(forUseMatch.Groups[2].Value.Substring(2, 2));
            
            var forUseEndTime = new DateTime(
                validTime.Year,
                validTime.Month,
                validTime.Day,
                endHour,
                endMinute,
                0,
                DateTimeKind.Utc);

            if (endHour == 0 && endMinute == 0)
            {
                forUseEndTime = forUseEndTime.AddDays(1);
            }

            return (forUseStartTime, forUseEndTime);
        }

        private class WindsAloftLevelData
        {
            public List<WindsAloftSite>? Sites { get; init; }
            public string Level { get; init; } = string.Empty;
        }

        private record WindsAloftSite
        {
            public string Id { get; init; } = string.Empty;
            public float Lat { get; init; }
            public float Lon { get; init; }
            public int Dir { get; init; }
            public int Spd { get; init; }
            public float? Temp { get; init; }
        }
    }
}