using System.Text.Json;
using Microsoft.Extensions.Logging;
using VFR3D.Infrastructure.Dtos.Navlog;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.Infrastructure.Services.WeatherServices
{
    public class WindsAloftService : IWindsAloftService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<WindsAloftService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public WindsAloftService(
            HttpClient httpClient,
            ILogger<WindsAloftService> logger)
        {
            _httpClient = httpClient;
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

                const string baseUrl = "https://aviationweather.gov/api/data/windtemp";
                
                // First, get the text response for valid times
                var textResponse = await _httpClient.GetStringAsync(
                    $"{baseUrl}?region=us&level=low&fcst={fcstHours:D2}");

                var validTime = ExtractValidTime(textResponse);
                var (forUseStartTime, forUseEndTime) = ExtractForUseTimes(textResponse);

                var altitudeLevels = new[] { 30, 60, 90, 120, 180, 240, 300, 340, 390 };
                var data = new List<WindsAloftSiteDto>();
                var airportDataMap = new Dictionary<string, WindsAloftSiteDto>();

                foreach (var level in altitudeLevels)
                {
                    var response = await _httpClient.GetStringAsync(
                        $"{baseUrl}?region=us&level={level}&fcst={fcstHours:D2}&format=json");
                    
                    var levelData = JsonSerializer.Deserialize<WindsAloftLevelData>(response, _jsonOptions);
                    if (levelData?.Sites == null) continue;

                    foreach (var site in levelData.Sites)
                    {
                        var altitude = (level * 100).ToString();

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

        private DateTime ExtractValidTime(string rawText)
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
            validTime = validTime.AddDays(day - validTime.Day); // Adjust to correct day
            validTime = validTime.Date.AddHours(hour).AddMinutes(minute);

            return validTime;
        }

        private (DateTime ForUseStartTime, DateTime ForUseEndTime) ExtractForUseTimes(string rawText)
        {
            var forUseMatch = System.Text.RegularExpressions.Regex.Match(rawText, @"FOR USE (\d{4})-(\d{4})");
            if (!forUseMatch.Success)
            {
                throw new FormatException("For use times not found in raw text");
            }

            var validTime = ExtractValidTime(rawText);
            
            var startHour = int.Parse(forUseMatch.Groups[1].Value[..2]);
            var startMinute = int.Parse(forUseMatch.Groups[1].Value.Substring(2, 2));
            var forUseStartTime = new DateTime(validTime.Year, validTime.Month, validTime.Day);

            if (startHour < validTime.Hour)
            {
                forUseStartTime = forUseStartTime.AddDays(1);
            }

            forUseStartTime = forUseStartTime.AddHours(startHour).AddMinutes(startMinute);

            var endHour = int.Parse(forUseMatch.Groups[2].Value[..2]);
            var endMinute = int.Parse(forUseMatch.Groups[2].Value.Substring(2, 2));
            var forUseEndTime = new DateTime(validTime.Year, validTime.Month, validTime.Day);

            if (endHour == 0 && endMinute == 0)
            {
                forUseEndTime = forUseEndTime.AddDays(1);
            }
            else if (endHour < startHour)
            {
                forUseEndTime = forUseEndTime.AddDays(1);
            }

            forUseEndTime = forUseEndTime.AddHours(endHour).AddMinutes(endMinute);

            return (forUseStartTime, forUseEndTime);
        }

        private class WindsAloftLevelData
        {
            public List<WindsAloftSite>? Sites { get; init; } = [];
            public int Level { get; init; }
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