using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using VFR3D.Infrastructure.Dtos.Navlog;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.Infrastructure.Services.WeatherServices
{
    public class WindsAloftService : IWindsAloftService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<WindsAloftService> _logger;

        // Standard altitude levels in feet
        private static readonly int[] AltitudeLevels = { 3000, 6000, 9000, 12000, 18000, 24000, 30000, 34000, 39000 };

        public WindsAloftService(
            IHttpClientFactory httpClientFactory,
            ILogger<WindsAloftService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
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
                    "winds aloft data");

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
                var sites = ParseWindsAloftText(textResponse);

                return new WindsAloftDto
                {
                    ValidTime = validTime,
                    ForUseStartTime = forUseStartTime,
                    ForUseEndTime = forUseEndTime,
                    WindTemp = sites
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching winds aloft data for forecast hours: {FcstHours}", fcstHours);
                throw;
            }
        }

        private List<WindsAloftSiteDto> ParseWindsAloftText(string textResponse)
        {
            var sites = new List<WindsAloftSiteDto>();
            var lines = textResponse.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                // Skip header lines and empty lines
                if (line.Length < 4 ||
                    line.StartsWith("FT") ||
                    line.StartsWith("DATA") ||
                    line.StartsWith("VALID") ||
                    line.StartsWith("FOR USE") ||
                    line.StartsWith("FB") ||
                    line.StartsWith("FD") ||
                    line.Contains("KWNO") ||
                    line.Contains("TEMPS NEG"))
                {
                    continue;
                }

                // Station ID is first 3 characters
                var stationId = line.Length >= 3 ? line[..3].Trim() : null;
                if (string.IsNullOrWhiteSpace(stationId) || !Regex.IsMatch(stationId, @"^[A-Z0-9]{2,3}$"))
                {
                    continue;
                }

                var windTemp = new Dictionary<string, WindTempDto>();

                // Parse wind data by splitting the line after station ID
                var dataSection = line.Length > 3 ? line[3..].TrimStart() : "";
                var tokens = dataSection.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                // Map tokens to altitudes based on position and format
                ParseWindTokens(tokens, windTemp);

                if (windTemp.Count > 0)
                {
                    var (lat, lon) = GetStationCoordinates(stationId);
                    sites.Add(new WindsAloftSiteDto
                    {
                        Id = stationId,
                        Lat = lat,
                        Lon = lon,
                        WindTemp = windTemp
                    });
                }
            }

            return sites;
        }

        private void ParseWindTokens(string[] tokens, Dictionary<string, WindTempDto> windTemp)
        {
            if (tokens.Length == 0) return;

            var tokenIndex = 0;
            var altitudeIndex = 0; // Index into AltitudeLevels array

            // Determine if first token is 3000 ft data (4 chars, no temp sign)
            // 3000 ft data is typically 4 chars without temp (just DDSS like "0508" or "9900")
            // 6000+ ft data has temperature (+/-) or is 6 chars at 30000+
            var firstToken = tokens[0];

            if (firstToken.Length == 4 && !firstToken.Contains('+') && !firstToken.Contains('-'))
            {
                // This looks like 3000 ft data (no temperature)
                var windData = ParseWindData(firstToken, 3000);
                if (windData != null)
                {
                    windTemp["3000"] = windData;
                }
                tokenIndex = 1;
                altitudeIndex = 1; // Next altitude is 6000 ft
            }
            else
            {
                // First token has temperature data, so it's 6000 ft (no 3000 ft data)
                altitudeIndex = 1; // Start at 6000 ft
            }

            // Parse remaining tokens for altitudes 6000 ft and up
            while (altitudeIndex < AltitudeLevels.Length && tokenIndex < tokens.Length)
            {
                var token = tokens[tokenIndex];
                var altitude = AltitudeLevels[altitudeIndex];

                var windData = ParseWindData(token, altitude);
                if (windData != null)
                {
                    windTemp[altitude.ToString()] = windData;
                }
                tokenIndex++;
                altitudeIndex++;
            }
        }

        private WindTempDto? ParseWindData(string token, int altitude)
        {
            if (string.IsNullOrWhiteSpace(token)) return null;

            // Light and variable (calm)
            if (token.StartsWith("9900"))
            {
                // Parse temperature if present
                float? temp = null;
                if (token.Length > 4)
                {
                    temp = ParseTemperature(token[4..], altitude);
                }
                return new WindTempDto
                {
                    Direction = null, // Light and variable
                    Speed = 0,
                    Temperature = temp
                };
            }

            // Must have at least 4 characters for direction and speed
            if (token.Length < 4) return null;

            // Parse direction (first 2 digits * 10) and speed (next 2 digits)
            if (!int.TryParse(token[..2], out var dirCode) || !int.TryParse(token.Substring(2, 2), out var speed))
            {
                return null;
            }

            int direction;
            // Handle high-speed encoding: if direction code > 36, add 50 to get actual direction/10
            // and add 100 to speed
            if (dirCode > 36)
            {
                direction = (dirCode - 50) * 10;
                speed += 100;
            }
            else
            {
                direction = dirCode * 10;
            }

            // Normalize direction
            if (direction == 0 || direction == 360)
            {
                direction = 360; // North
            }

            // Parse temperature if present
            float? temperature = null;
            if (token.Length > 4)
            {
                temperature = ParseTemperature(token[4..], altitude);
            }

            return new WindTempDto
            {
                Direction = direction,
                Speed = speed,
                Temperature = temperature
            };
        }

        private static float? ParseTemperature(string tempPart, int altitude)
        {
            if (string.IsNullOrWhiteSpace(tempPart)) return null;

            // At 30000 ft and above, temperatures are always negative and don't have a sign
            if (altitude >= 30000)
            {
                if (int.TryParse(tempPart, out var highAltTemp))
                {
                    return -highAltTemp;
                }
                return null;
            }

            // Below 30000 ft, temperature has a +/- sign
            if (tempPart.StartsWith('+'))
            {
                if (int.TryParse(tempPart[1..], out var temp))
                {
                    return temp;
                }
            }
            else if (tempPart.StartsWith('-'))
            {
                if (int.TryParse(tempPart[1..], out var temp))
                {
                    return -temp;
                }
            }
            else if (int.TryParse(tempPart, out var temp))
            {
                // No sign - assume positive for lower altitudes
                return temp;
            }

            return null;
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
            var validTimeMatch = Regex.Match(rawText, @"VALID (\d{6})Z?");
            if (!validTimeMatch.Success)
            {
                throw new FormatException("Valid time not found in raw text");
            }

            var validTimeString = validTimeMatch.Groups[1].Value;
            var day = int.Parse(validTimeString[..2]);
            var hour = int.Parse(validTimeString.Substring(2, 2));
            var minute = int.Parse(validTimeString.Substring(4, 2));

            var now = DateTime.UtcNow;
            var validTime = new DateTime(now.Year, now.Month, day, hour, minute, 0, DateTimeKind.Utc);

            // Handle month boundary - if day is greater than current day and we're early in the month,
            // the valid time might be from the previous month
            if (day > now.Day + 7)
            {
                validTime = validTime.AddMonths(-1);
            }
            // If day is much less than current day, it might be next month
            else if (day < now.Day - 7)
            {
                validTime = validTime.AddMonths(1);
            }

            return validTime;
        }

        private static (DateTime ForUseStartTime, DateTime ForUseEndTime) ExtractForUseTimes(string rawText)
        {
            var forUseMatch = Regex.Match(rawText, @"FOR USE (\d{4})-(\d{4})Z?\.?");
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

            // If start hour is greater than valid hour, it's from the previous day
            if (startHour > validTime.Hour + 6)
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

            // Handle day boundary - if end time is early morning (0000-0600), it's next day
            if (endHour < startHour || (endHour == 0 && endMinute == 0))
            {
                forUseEndTime = forUseEndTime.AddDays(1);
            }

            return (forUseStartTime, forUseEndTime);
        }

        /// <summary>
        /// Returns approximate lat/lon coordinates for winds aloft reporting stations.
        /// These are FAA winds aloft forecast locations across the US.
        /// </summary>
        private static (float Lat, float Lon) GetStationCoordinates(string stationId)
        {
            // Comprehensive dictionary of US winds aloft station coordinates
            var stationCoordinates = new Dictionary<string, (float Lat, float Lon)>
            {
                // Major airports and VORs
                { "ABI", (32.41f, -99.68f) },   // Abilene, TX
                { "ABQ", (35.04f, -106.61f) },  // Albuquerque, NM
                { "ABR", (45.45f, -98.42f) },   // Aberdeen, SD
                { "ACK", (41.25f, -70.06f) },   // Nantucket, MA
                { "ACY", (39.46f, -74.58f) },   // Atlantic City, NJ
                { "AGC", (40.35f, -79.93f) },   // Pittsburgh Allegheny, PA
                { "ALB", (42.75f, -73.80f) },   // Albany, NY
                { "ALS", (37.43f, -105.87f) },  // Alamosa, CO
                { "AMA", (35.22f, -101.71f) },  // Amarillo, TX
                { "AST", (46.16f, -123.88f) },  // Astoria, OR
                { "ATL", (33.64f, -84.43f) },   // Atlanta, GA
                { "AVP", (41.34f, -75.73f) },   // Wilkes-Barre, PA
                { "AXN", (45.87f, -95.39f) },   // Alexandria, MN
                { "BAM", (40.57f, -116.92f) },  // Battle Mountain, NV
                { "BCE", (37.69f, -112.30f) },  // Bryce Canyon, UT
                { "BDL", (41.94f, -72.68f) },   // Hartford, CT
                { "BFF", (41.87f, -103.60f) },  // Scottsbluff, NE
                { "BGR", (44.81f, -68.83f) },   // Bangor, ME
                { "BHM", (33.56f, -86.75f) },   // Birmingham, AL
                { "BIH", (37.37f, -118.36f) },  // Bishop, CA
                { "BIL", (45.81f, -108.54f) },  // Billings, MT
                { "BLH", (33.62f, -114.72f) },  // Blythe, CA
                { "BML", (44.58f, -71.18f) },   // Berlin, NH
                { "BNA", (36.12f, -86.68f) },   // Nashville, TN
                { "BOI", (43.56f, -116.22f) },  // Boise, ID
                { "BOS", (42.36f, -71.01f) },   // Boston, MA
                { "BRL", (40.78f, -91.13f) },   // Burlington, IA
                { "BRO", (25.91f, -97.43f) },   // Brownsville, TX
                { "BUF", (42.94f, -78.74f) },   // Buffalo, NY
                { "CAE", (33.94f, -81.12f) },   // Columbia, SC
                { "CAR", (46.87f, -68.02f) },   // Caribou, ME
                { "CGI", (37.23f, -89.57f) },   // Cape Girardeau, MO
                { "CHS", (32.90f, -80.04f) },   // Charleston, SC
                { "CLE", (41.41f, -81.85f) },   // Cleveland, OH
                { "CLL", (30.59f, -96.36f) },   // College Station, TX
                { "CMH", (40.00f, -82.89f) },   // Columbus, OH
                { "COU", (38.82f, -92.22f) },   // Columbia, MO
                { "CRP", (27.77f, -97.50f) },   // Corpus Christi, TX
                { "CRW", (38.37f, -81.59f) },   // Charleston, WV
                { "CSG", (32.52f, -84.94f) },   // Columbus, GA
                { "CVG", (39.05f, -84.66f) },   // Cincinnati, OH
                { "CZI", (35.02f, -110.79f) },  // Crazy Woman, WY
                { "DAL", (32.85f, -96.85f) },   // Dallas, TX
                { "DBQ", (42.40f, -90.71f) },   // Dubuque, IA
                { "DEN", (39.86f, -104.67f) },  // Denver, CO
                { "DIK", (46.80f, -102.80f) },  // Dickinson, ND
                { "DLH", (46.84f, -92.19f) },   // Duluth, MN
                { "DLN", (45.25f, -112.55f) },  // Dillon, MT
                { "DRT", (29.37f, -100.93f) },  // Del Rio, TX
                { "DSM", (41.53f, -93.66f) },   // Des Moines, IA
                { "ECK", (43.26f, -82.72f) },   // Peck, MI
                { "EKN", (38.89f, -79.86f) },   // Elkins, WV
                { "ELP", (31.81f, -106.38f) },  // El Paso, TX
                { "ELY", (39.30f, -114.84f) },  // Ely, NV
                { "EMI", (39.49f, -76.98f) },   // Westminster, MD
                { "EVV", (38.04f, -87.53f) },   // Evansville, IN
                { "EYW", (24.56f, -81.76f) },   // Key West, FL
                { "FAT", (36.78f, -119.72f) },  // Fresno, CA
                { "FLO", (34.19f, -79.72f) },   // Florence, SC
                { "FMN", (36.74f, -108.23f) },  // Farmington, NM
                { "FOT", (40.55f, -124.13f) },  // Fortuna, CA
                { "FSD", (43.58f, -96.74f) },   // Sioux Falls, SD
                { "FSM", (35.34f, -94.37f) },   // Fort Smith, AR
                { "FWA", (41.01f, -85.19f) },   // Fort Wayne, IN
                { "GAG", (36.30f, -99.77f) },   // Gage, OK
                { "GCK", (37.93f, -100.72f) },  // Garden City, KS
                { "GEG", (47.62f, -117.53f) },  // Spokane, WA
                { "GFK", (47.95f, -97.18f) },   // Grand Forks, ND
                { "GGW", (48.21f, -106.62f) },  // Glasgow, MT
                { "GJT", (39.12f, -108.53f) },  // Grand Junction, CO
                { "GLD", (39.37f, -101.70f) },  // Goodland, KS
                { "GPI", (48.31f, -114.26f) },  // Kalispell, MT (Glacier Park)
                { "GRB", (44.49f, -88.13f) },   // Green Bay, WI
                { "GRI", (40.97f, -98.31f) },   // Grand Island, NE
                { "GSP", (34.88f, -82.22f) },   // Greenville-Spartanburg, SC
                { "GTF", (47.48f, -111.37f) },  // Great Falls, MT
                { "HAT", (35.26f, -75.55f) },   // Cape Hatteras, NC
                { "HOU", (29.65f, -95.28f) },   // Houston, TX
                { "HSV", (34.64f, -86.77f) },   // Huntsville, AL
                { "ICT", (37.65f, -97.43f) },   // Wichita, KS
                { "ILM", (34.27f, -77.90f) },   // Wilmington, NC
                { "IMB", (33.15f, -114.80f) },  // Imperial, CA
                { "IND", (39.72f, -86.27f) },   // Indianapolis, IN
                { "INK", (31.78f, -103.20f) },  // Wink, TX
                { "INL", (48.57f, -93.40f) },   // International Falls, MN
                { "JAN", (32.31f, -90.08f) },   // Jackson, MS
                { "JAX", (30.49f, -81.69f) },   // Jacksonville, FL
                { "JFK", (40.64f, -73.78f) },   // New York JFK, NY
                { "JOT", (41.52f, -88.18f) },   // Joliet, IL
                { "LAS", (36.08f, -115.15f) },  // Las Vegas, NV
                { "LBB", (33.66f, -101.82f) },  // Lubbock, TX
                { "LCH", (30.13f, -93.22f) },   // Lake Charles, LA
                { "LIT", (34.73f, -92.22f) },   // Little Rock, AR
                { "LKV", (42.16f, -120.40f) },  // Lakeview, OR
                { "LND", (42.82f, -108.73f) },  // Lander, WY
                { "LOU", (38.23f, -85.66f) },   // Louisville, KY
                { "LRD", (27.54f, -99.46f) },   // Laredo, TX
                { "LSE", (43.88f, -91.26f) },   // La Crosse, WI
                { "LWS", (46.37f, -117.01f) },  // Lewiston, ID
                { "MBW", (41.13f, -100.68f) },  // North Platte, NE
                { "MCW", (43.16f, -93.33f) },   // Mason City, IA
                { "MEM", (35.06f, -89.98f) },   // Memphis, TN
                { "MGM", (32.30f, -86.39f) },   // Montgomery, AL
                { "MIA", (25.79f, -80.29f) },   // Miami, FL
                { "MKC", (39.12f, -94.59f) },   // Kansas City, MO
                { "MKG", (43.17f, -86.24f) },   // Muskegon, MI
                { "MLB", (28.10f, -80.64f) },   // Melbourne, FL
                { "MLS", (46.43f, -105.96f) },  // Miles City, MT
                { "MOB", (30.69f, -88.24f) },   // Mobile, AL
                { "MOT", (48.26f, -101.28f) },  // Minot, ND
                { "MQT", (46.53f, -87.56f) },   // Marquette, MI
                { "MRF", (30.37f, -104.02f) },  // Marfa, TX
                { "MSP", (44.88f, -93.22f) },   // Minneapolis, MN
                { "MSY", (29.99f, -90.26f) },   // New Orleans, LA
                { "OKC", (35.39f, -97.60f) },   // Oklahoma City, OK
                { "OMA", (41.30f, -95.89f) },   // Omaha, NE
                { "ONL", (42.47f, -98.69f) },   // O'Neill, NE
                { "ONT", (34.06f, -117.60f) },  // Ontario, CA
                { "ORF", (36.90f, -76.19f) },   // Norfolk, VA
                { "OTH", (43.42f, -124.25f) },  // North Bend, OR
                { "PDX", (45.59f, -122.60f) },  // Portland, OR
                { "PFN", (30.21f, -85.68f) },   // Panama City, FL
                { "PHX", (33.43f, -112.01f) },  // Phoenix, AZ
                { "PIE", (27.91f, -82.69f) },   // St. Petersburg, FL
                { "PIH", (42.91f, -112.60f) },  // Pocatello, ID
                { "PIR", (44.38f, -100.29f) },  // Pierre, SD
                { "PLB", (44.69f, -73.52f) },   // Plattsburgh, NY
                { "PRC", (34.65f, -112.42f) },  // Prescott, AZ
                { "PSB", (40.92f, -77.98f) },   // Philipsburg, PA
                { "PSX", (28.73f, -96.25f) },   // Palacios, TX
                { "PUB", (38.29f, -104.50f) },  // Pueblo, CO
                { "PWM", (43.65f, -70.31f) },   // Portland, ME
                { "RAP", (44.05f, -103.05f) },  // Rapid City, SD
                { "RBL", (40.10f, -122.24f) },  // Red Bluff, CA
                { "RDM", (44.25f, -121.15f) },  // Redmond, OR
                { "RDU", (35.88f, -78.79f) },   // Raleigh-Durham, NC
                { "RIC", (37.51f, -77.32f) },   // Richmond, VA
                { "RKS", (41.60f, -109.07f) },  // Rock Springs, WY
                { "RNO", (39.50f, -119.77f) },  // Reno, NV
                { "ROA", (37.32f, -79.98f) },   // Roanoke, VA
                { "ROW", (33.30f, -104.53f) },  // Roswell, NM
                { "SAC", (38.51f, -121.49f) },  // Sacramento, CA
                { "SAN", (32.73f, -117.19f) },  // San Diego, CA
                { "SAT", (29.53f, -98.47f) },   // San Antonio, TX
                { "SAV", (32.13f, -81.20f) },   // Savannah, GA
                { "SBA", (34.43f, -119.84f) },  // Santa Barbara, CA
                { "SEA", (47.45f, -122.31f) },  // Seattle, WA
                { "SFO", (37.62f, -122.37f) },  // San Francisco, CA
                { "SGF", (37.24f, -93.39f) },   // Springfield, MO
                { "SHV", (32.45f, -93.83f) },   // Shreveport, LA
                { "SIY", (41.78f, -122.47f) },  // Montague, CA
                { "SLC", (40.79f, -111.98f) },  // Salt Lake City, UT
                { "SLN", (38.79f, -97.65f) },   // Salina, KS
                { "SPI", (39.84f, -89.68f) },   // Springfield, IL
                { "SPS", (33.99f, -98.49f) },   // Wichita Falls, TX
                { "SSM", (46.41f, -84.31f) },   // Sault Ste. Marie, MI
                { "STL", (38.75f, -90.36f) },   // St. Louis, MO
                { "SYR", (43.11f, -76.11f) },   // Syracuse, NY
                { "TCC", (35.18f, -103.60f) },  // Tucumcari, NM
                { "TLH", (30.40f, -84.35f) },   // Tallahassee, FL
                { "TRI", (36.48f, -82.40f) },   // Tri-Cities, TN
                { "TUL", (36.20f, -95.89f) },   // Tulsa, OK
                { "TUS", (32.12f, -110.94f) },  // Tucson, AZ
                { "TVC", (44.74f, -85.58f) },   // Traverse City, MI
                { "TYS", (35.81f, -83.99f) },   // Knoxville, TN
                { "YKM", (46.57f, -120.44f) },  // Yakima, WA
                { "YUM", (32.66f, -114.61f) },  // Yuma, AZ
                // Additional stations for Hawaii/Pacific
                { "H51", (21.32f, -157.92f) },  // Honolulu, HI
                { "H52", (19.72f, -155.05f) },  // Hilo, HI
                { "H61", (20.79f, -156.43f) },  // Kahului, HI
                { "T01", (18.43f, -66.00f) },   // San Juan, PR
                { "T06", (18.46f, -67.14f) },   // Mayaguez, PR
                { "T07", (18.26f, -65.64f) },   // Roosevelt Roads, PR
            };

            if (stationCoordinates.TryGetValue(stationId, out var coords))
            {
                return coords;
            }

            // Return 0,0 for unknown stations - the NavlogService will fall back to ID matching
            return (0f, 0f);
        }
    }
}