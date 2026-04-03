using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RichardSzalay.MockHttp;
using VFR3D.Infrastructure.Services.WeatherServices;
using Xunit;

namespace VFR3D.Tests.WeatherServicesTests
{
    public class WindsAloftServiceTests
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<WindsAloftService> _logger;
        private readonly WindsAloftService _windsAloftService;
        private readonly MockHttpMessageHandler _mockHttp;

        private const string BaseUrl = "https://aviationweather.gov/api/data/windtemp";

        private const string MockRawTextResponse = @"000
FBUS31 KWNO 061958
FD1US1
DATA BASED ON 061800Z
VALID 070000Z   FOR USE 2000-0300Z. TEMPS NEG ABV 24000

FT  3000    6000    9000   12000   18000   24000  30000  34000  39000
ABI      1942+11 2430+11 2538+03 2564-13 2585-22 750237 751843 268947
ABQ              2438+02 2441-07 2571-18 2488-29 742538 743840 742844
ABR 0508 3510-05 3017-08 2925-14 2841-26 2768-33 268850 278952 278346";

        public WindsAloftServiceTests()
        {
            _httpClientFactory = Substitute.For<IHttpClientFactory>();
            _logger = Substitute.For<ILogger<WindsAloftService>>();
            _mockHttp = new MockHttpMessageHandler();
            var client = _mockHttp.ToHttpClient();
            _httpClientFactory.CreateClient(Arg.Any<string>()).Returns(client);
            _windsAloftService = new WindsAloftService(_httpClientFactory, _logger);
        }

        #region Forecast Hours Validation Tests

        [Fact]
        public async Task FetchWindsAloftData_ShouldThrowArgumentException_WhenFcstHoursIsInvalid()
        {
            // Arrange
            var invalidFcstHours = 5;

            // Act
            Func<Task> act = async () => await _windsAloftService.FetchWindsAloftData(invalidFcstHours);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Forecast hours must be 6, 12, or 24 (Parameter 'fcstHours')");
        }

        [Theory]
        [InlineData(6)]
        [InlineData(12)]
        [InlineData(24)]
        public async Task FetchWindsAloftData_ShouldAcceptValidForecastHours(int fcstHours)
        {
            // Arrange
            _mockHttp.When($"{BaseUrl}?region=us&level=low&fcst={fcstHours:D2}")
                     .Respond("text/plain", MockRawTextResponse);

            // Act
            var result = await _windsAloftService.FetchWindsAloftData(fcstHours);

            // Assert
            result.Should().NotBeNull();
            result.WindTemp.Should().NotBeEmpty();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(7)]
        [InlineData(18)]
        [InlineData(48)]
        [InlineData(-6)]
        public async Task FetchWindsAloftData_ShouldRejectInvalidForecastHours(int fcstHours)
        {
            // Act
            Func<Task> act = async () => await _windsAloftService.FetchWindsAloftData(fcstHours);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        #endregion

        #region HTTP Status Code Tests

        [Fact]
        public async Task FetchWindsAloftData_ShouldReturnEmptyDto_When204NoContent()
        {
            // Arrange
            _mockHttp.When($"{BaseUrl}?region=us&level=low&fcst=06")
                     .Respond(HttpStatusCode.NoContent);

            // Act
            var result = await _windsAloftService.FetchWindsAloftData(6);

            // Assert
            result.Should().NotBeNull();
            result.WindTemp.Should().BeEmpty();
            result.ValidTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        }

        [Fact]
        public async Task FetchWindsAloftData_ShouldThrowHttpRequestException_When400BadRequest()
        {
            // Arrange
            _mockHttp.When($"{BaseUrl}?region=us&level=low&fcst=06")
                     .Respond(HttpStatusCode.BadRequest);

            // Act
            Func<Task> act = async () => await _windsAloftService.FetchWindsAloftData(6);

            // Assert
            await act.Should().ThrowAsync<HttpRequestException>()
                .WithMessage("*400*");
        }

        [Fact]
        public async Task FetchWindsAloftData_ShouldThrowHttpRequestException_When404NotFound()
        {
            // Arrange
            _mockHttp.When($"{BaseUrl}?region=us&level=low&fcst=06")
                     .Respond(HttpStatusCode.NotFound);

            // Act
            Func<Task> act = async () => await _windsAloftService.FetchWindsAloftData(6);

            // Assert
            await act.Should().ThrowAsync<HttpRequestException>()
                .WithMessage("*404*");
        }

        [Fact]
        public async Task FetchWindsAloftData_ShouldThrowHttpRequestException_When429TooManyRequests()
        {
            // Arrange
            _mockHttp.When($"{BaseUrl}?region=us&level=low&fcst=06")
                     .Respond(HttpStatusCode.TooManyRequests);

            // Act
            Func<Task> act = async () => await _windsAloftService.FetchWindsAloftData(6);

            // Assert
            await act.Should().ThrowAsync<HttpRequestException>()
                .WithMessage("*429*");
        }

        [Fact]
        public async Task FetchWindsAloftData_ShouldThrowHttpRequestException_When500InternalServerError()
        {
            // Arrange
            _mockHttp.When($"{BaseUrl}?region=us&level=low&fcst=06")
                     .Respond(HttpStatusCode.InternalServerError);

            // Act
            Func<Task> act = async () => await _windsAloftService.FetchWindsAloftData(6);

            // Assert
            await act.Should().ThrowAsync<HttpRequestException>()
                .WithMessage("*500*");
        }

        [Fact]
        public async Task FetchWindsAloftData_ShouldThrowHttpRequestException_When502BadGateway()
        {
            // Arrange
            _mockHttp.When($"{BaseUrl}?region=us&level=low&fcst=06")
                     .Respond(HttpStatusCode.BadGateway);

            // Act
            Func<Task> act = async () => await _windsAloftService.FetchWindsAloftData(6);

            // Assert
            await act.Should().ThrowAsync<HttpRequestException>()
                .WithMessage("*502*");
        }

        [Fact]
        public async Task FetchWindsAloftData_ShouldThrowHttpRequestException_When504GatewayTimeout()
        {
            // Arrange
            _mockHttp.When($"{BaseUrl}?region=us&level=low&fcst=06")
                     .Respond(HttpStatusCode.GatewayTimeout);

            // Act
            Func<Task> act = async () => await _windsAloftService.FetchWindsAloftData(6);

            // Assert
            await act.Should().ThrowAsync<HttpRequestException>()
                .WithMessage("*504*");
        }

        #endregion

        #region Wind Data Parsing Tests

        [Fact]
        public async Task FetchWindsAloftData_ShouldReturnWindsAloftDto_WhenFcstHoursIsValid()
        {
            // Arrange
            var validFcstHours = 6;
            _mockHttp.When($"{BaseUrl}?region=us&level=low&fcst={validFcstHours:D2}")
                     .Respond("text/plain", MockRawTextResponse);

            // Act
            var result = await _windsAloftService.FetchWindsAloftData(validFcstHours);

            // Assert
            result.Should().NotBeNull();
            // Valid time should be day 07 at 0000Z based on "VALID 070000Z"
            result.ValidTime.Day.Should().Be(7);
            result.ValidTime.Hour.Should().Be(0);
            result.ValidTime.Minute.Should().Be(0);
            // For use start time should be 2000Z based on "FOR USE 2000-0300Z"
            result.ForUseStartTime.Hour.Should().Be(20);
            result.ForUseStartTime.Minute.Should().Be(0);
            // For use end time should be 0300Z (next day)
            result.ForUseEndTime.Hour.Should().Be(3);
            result.ForUseEndTime.Minute.Should().Be(0);

            result.WindTemp.Should().NotBeEmpty();
            result.WindTemp.Should().HaveCount(3); // ABI, ABQ, ABR

            // Check ABI station - no 3000 ft data, starts at 6000 ft
            var abi = result.WindTemp.First(s => s.Id == "ABI");
            abi.Should().NotBeNull();
            abi.Lat.Should().BeApproximately(32.41f, 0.1f); // From static coordinates
            abi.Lon.Should().BeApproximately(-99.68f, 0.1f);
            abi.WindTemp.Should().NotContainKey("3000"); // ABI has no 3000 ft data
            abi.WindTemp.Should().ContainKey("6000");
            abi.WindTemp["6000"].Direction.Should().Be(190); // 19 * 10 = 190
            abi.WindTemp["6000"].Speed.Should().Be(42);
            abi.WindTemp["6000"].Temperature.Should().Be(11);

            // Check ABR station - has 3000 ft data
            var abr = result.WindTemp.First(s => s.Id == "ABR");
            abr.Should().NotBeNull();
            abr.WindTemp.Should().ContainKey("3000");
            abr.WindTemp["3000"].Direction.Should().Be(50); // 05 * 10 = 50
            abr.WindTemp["3000"].Speed.Should().Be(8);
            abr.WindTemp["3000"].Temperature.Should().BeNull(); // No temp at 3000 ft for "0508"
        }

        [Fact]
        public async Task FetchWindsAloftData_ShouldParseHighSpeedWinds()
        {
            // High speed encoding: direction code > 36 means add 50 to get actual dir/10 and add 100 to speed
            // Example: "7506" = (75-50)*10 = 250° at 6+100 = 106 knots
            var responseWithHighSpeedWinds = @"000
FBUS31 KWNO 211800
FD1US1
DATA BASED ON 211800Z
VALID 220000Z   FOR USE 2000-0300Z. TEMPS NEG ABV 24000

FT  3000    6000    9000   12000   18000   24000  30000  34000  39000
TST 7512 7525-10 6230-15 5835-20 5540-25 5245-30 520635 521040 521545";

            _mockHttp.When($"{BaseUrl}?region=us&level=low&fcst=06")
                     .Respond("text/plain", responseWithHighSpeedWinds);

            // Act
            var result = await _windsAloftService.FetchWindsAloftData(6);

            // Assert
            var tst = result.WindTemp.First(s => s.Id == "TST");

            // 3000 ft: "7512" = (75-50)*10 = 250° at 12+100 = 112 kt
            tst.WindTemp["3000"].Direction.Should().Be(250);
            tst.WindTemp["3000"].Speed.Should().Be(112);

            // 6000 ft: "7525-10" = 250° at 125 kt, -10°C
            tst.WindTemp["6000"].Direction.Should().Be(250);
            tst.WindTemp["6000"].Speed.Should().Be(125);
            tst.WindTemp["6000"].Temperature.Should().Be(-10);

            // 9000 ft: "6230-15" = (62-50)*10 = 120° at 30+100 = 130 kt, -15°C
            tst.WindTemp["9000"].Direction.Should().Be(120);
            tst.WindTemp["9000"].Speed.Should().Be(130);
            tst.WindTemp["9000"].Temperature.Should().Be(-15);
        }

        [Fact]
        public async Task FetchWindsAloftData_ShouldParseLightAndVariableWinds()
        {
            // "9900" indicates light and variable winds (calm)
            var responseWithLightWinds = @"000
FBUS31 KWNO 211800
FD1US1
DATA BASED ON 211800Z
VALID 220000Z   FOR USE 2000-0300Z. TEMPS NEG ABV 24000

FT  3000    6000    9000   12000   18000   24000  30000  34000  39000
LAX 9900 9900+15 9900+10 2520+05 2530-10 2540-20 254030 254535 255040";

            _mockHttp.When($"{BaseUrl}?region=us&level=low&fcst=06")
                     .Respond("text/plain", responseWithLightWinds);

            // Act
            var result = await _windsAloftService.FetchWindsAloftData(6);

            // Assert
            var lax = result.WindTemp.First(s => s.Id == "LAX");

            // 3000 ft: "9900" = light and variable
            lax.WindTemp["3000"].Direction.Should().BeNull();
            lax.WindTemp["3000"].Speed.Should().Be(0);

            // 6000 ft: "9900+15" = light and variable, +15°C
            lax.WindTemp["6000"].Direction.Should().BeNull();
            lax.WindTemp["6000"].Speed.Should().Be(0);
            lax.WindTemp["6000"].Temperature.Should().Be(15);

            // 12000 ft: normal wind data "2520+05"
            lax.WindTemp["12000"].Direction.Should().Be(250);
            lax.WindTemp["12000"].Speed.Should().Be(20);
            lax.WindTemp["12000"].Temperature.Should().Be(5);
        }

        [Fact]
        public async Task FetchWindsAloftData_ShouldParseHighAltitudeTemperatures()
        {
            // At 30000 ft and above, temperatures are always negative (no sign in data)
            // Format: DDSSTT where TT is always negative
            var response = @"000
FBUS31 KWNO 211800
FD1US1
DATA BASED ON 211800Z
VALID 220000Z   FOR USE 2000-0300Z. TEMPS NEG ABV 24000

FT  3000    6000    9000   12000   18000   24000  30000  34000  39000
DEN      2520+10 2525+05 2530-05 2535-15 2540-25 254035 254540 255045";

            _mockHttp.When($"{BaseUrl}?region=us&level=low&fcst=06")
                     .Respond("text/plain", response);

            // Act
            var result = await _windsAloftService.FetchWindsAloftData(6);

            // Assert
            var den = result.WindTemp.First(s => s.Id == "DEN");

            // 30000 ft: "254035" = 250° at 40 kt, -35°C (temp always negative)
            den.WindTemp["30000"].Direction.Should().Be(250);
            den.WindTemp["30000"].Speed.Should().Be(40);
            den.WindTemp["30000"].Temperature.Should().Be(-35);

            // 34000 ft: "254540" = 250° at 45 kt, -40°C
            den.WindTemp["34000"].Direction.Should().Be(250);
            den.WindTemp["34000"].Speed.Should().Be(45);
            den.WindTemp["34000"].Temperature.Should().Be(-40);

            // 39000 ft: "255045" = 250° at 50 kt, -45°C
            den.WindTemp["39000"].Direction.Should().Be(250);
            den.WindTemp["39000"].Speed.Should().Be(50);
            den.WindTemp["39000"].Temperature.Should().Be(-45);
        }

        [Fact]
        public async Task FetchWindsAloftData_ShouldParseNegativeTemperaturesAtLowerAltitudes()
        {
            var response = @"000
FBUS31 KWNO 211800
FD1US1
DATA BASED ON 211800Z
VALID 220000Z   FOR USE 2000-0300Z. TEMPS NEG ABV 24000

FT  3000    6000    9000   12000   18000   24000  30000  34000  39000
MSP      2520-05 2525-10 2530-15 2535-25 2540-35 254045 254550 255055";

            _mockHttp.When($"{BaseUrl}?region=us&level=low&fcst=06")
                     .Respond("text/plain", response);

            // Act
            var result = await _windsAloftService.FetchWindsAloftData(6);

            // Assert
            var msp = result.WindTemp.First(s => s.Id == "MSP");

            // Verify negative temperatures at lower altitudes
            msp.WindTemp["6000"].Temperature.Should().Be(-5);
            msp.WindTemp["9000"].Temperature.Should().Be(-10);
            msp.WindTemp["12000"].Temperature.Should().Be(-15);
            msp.WindTemp["18000"].Temperature.Should().Be(-25);
            msp.WindTemp["24000"].Temperature.Should().Be(-35);
        }

        [Fact]
        public async Task FetchWindsAloftData_ShouldParsePositiveTemperatures()
        {
            var response = @"000
FBUS31 KWNO 211800
FD1US1
DATA BASED ON 211800Z
VALID 220000Z   FOR USE 2000-0300Z. TEMPS NEG ABV 24000

FT  3000    6000    9000   12000   18000   24000  30000  34000  39000
MIA      2520+25 2525+20 2530+15 2535+05 2540-10 254030 254535 255040";

            _mockHttp.When($"{BaseUrl}?region=us&level=low&fcst=06")
                     .Respond("text/plain", response);

            // Act
            var result = await _windsAloftService.FetchWindsAloftData(6);

            // Assert
            var mia = result.WindTemp.First(s => s.Id == "MIA");

            // Verify positive temperatures
            mia.WindTemp["6000"].Temperature.Should().Be(25);
            mia.WindTemp["9000"].Temperature.Should().Be(20);
            mia.WindTemp["12000"].Temperature.Should().Be(15);
            mia.WindTemp["18000"].Temperature.Should().Be(5);
        }

        [Fact]
        public async Task FetchWindsAloftData_ShouldHandleMissing3000FtData()
        {
            // ABQ has no 3000 ft data (starts at 6000 with temperature)
            // Note: When parsing by whitespace, we detect missing 3000 ft when first token has temp sign
            var response = @"000
FBUS31 KWNO 211800
FD1US1
DATA BASED ON 211800Z
VALID 220000Z   FOR USE 2000-0300Z. TEMPS NEG ABV 24000

FT  3000    6000    9000   12000   18000   24000  30000  34000  39000
ABQ      2438+02 2441-07 2571-18 2488-29 742538 743840 742844 742850";

            _mockHttp.When($"{BaseUrl}?region=us&level=low&fcst=06")
                     .Respond("text/plain", response);

            // Act
            var result = await _windsAloftService.FetchWindsAloftData(6);

            // Assert
            var abq = result.WindTemp.First(s => s.Id == "ABQ");
            abq.WindTemp.Should().NotContainKey("3000"); // No 3000 ft data
            abq.WindTemp.Should().ContainKey("6000");    // Starts at 6000 ft
            abq.WindTemp["6000"].Direction.Should().Be(240);
            abq.WindTemp["6000"].Speed.Should().Be(38);
            abq.WindTemp["6000"].Temperature.Should().Be(2);
        }

        [Fact]
        public async Task FetchWindsAloftData_ShouldParseNorthWindCorrectly()
        {
            // Direction 36 (360°) should be normalized to 360 (North)
            var response = @"000
FBUS31 KWNO 211800
FD1US1
DATA BASED ON 211800Z
VALID 220000Z   FOR USE 2000-0300Z. TEMPS NEG ABV 24000

FT  3000    6000    9000   12000   18000   24000  30000  34000  39000
ORD 3620 3625+10 3630+05 3635-05 0020-15 0025-25 002535 003040 003545";

            _mockHttp.When($"{BaseUrl}?region=us&level=low&fcst=06")
                     .Respond("text/plain", response);

            // Act
            var result = await _windsAloftService.FetchWindsAloftData(6);

            // Assert
            var ord = result.WindTemp.First(s => s.Id == "ORD");

            // 3000 ft: "3620" = 360° (North) at 20 kt
            ord.WindTemp["3000"].Direction.Should().Be(360);
            ord.WindTemp["3000"].Speed.Should().Be(20);

            // 18000 ft: "0020-15" = 0° which should be 360° (North) at 20 kt
            ord.WindTemp["18000"].Direction.Should().Be(360);
        }

        #endregion

        #region Station Coordinate Tests

        [Fact]
        public async Task FetchWindsAloftData_ShouldReturnKnownStationCoordinates()
        {
            _mockHttp.When($"{BaseUrl}?region=us&level=low&fcst=06")
                     .Respond("text/plain", MockRawTextResponse);

            // Act
            var result = await _windsAloftService.FetchWindsAloftData(6);

            // Assert - Check known stations have correct coordinates
            var abi = result.WindTemp.First(s => s.Id == "ABI");
            abi.Lat.Should().BeApproximately(32.41f, 0.1f);
            abi.Lon.Should().BeApproximately(-99.68f, 0.1f);

            var abq = result.WindTemp.First(s => s.Id == "ABQ");
            abq.Lat.Should().BeApproximately(35.04f, 0.1f);
            abq.Lon.Should().BeApproximately(-106.61f, 0.1f);

            var abr = result.WindTemp.First(s => s.Id == "ABR");
            abr.Lat.Should().BeApproximately(45.45f, 0.1f);
            abr.Lon.Should().BeApproximately(-98.42f, 0.1f);
        }

        [Fact]
        public async Task FetchWindsAloftData_ShouldReturnZeroCoordinatesForUnknownStations()
        {
            var responseWithUnknownStation = @"000
FBUS31 KWNO 211800
FD1US1
DATA BASED ON 211800Z
VALID 220000Z   FOR USE 2000-0300Z. TEMPS NEG ABV 24000

FT  3000    6000    9000   12000   18000   24000  30000  34000  39000
XYZ 1520 2025+10 2530+05 3035-05 3540-15 0045-25 004535 005040 005545";

            _mockHttp.When($"{BaseUrl}?region=us&level=low&fcst=06")
                     .Respond("text/plain", responseWithUnknownStation);

            // Act
            var result = await _windsAloftService.FetchWindsAloftData(6);

            // Assert - Unknown station should have 0,0 coordinates
            var xyz = result.WindTemp.First(s => s.Id == "XYZ");
            xyz.Lat.Should().Be(0f);
            xyz.Lon.Should().Be(0f);
        }

        #endregion

        #region Time Parsing Tests

        [Fact]
        public async Task FetchWindsAloftData_ShouldParseValidTimeWithZSuffix()
        {
            var response = @"000
FBUS31 KWNO 211800
FD1US1
DATA BASED ON 211800Z
VALID 221200Z   FOR USE 0900-1500Z. TEMPS NEG ABV 24000

FT  3000    6000    9000   12000   18000   24000  30000  34000  39000
ABI 1520 2025+10 2530+05 3035-05 3540-15 0045-25 004535 005040 005545";

            _mockHttp.When($"{BaseUrl}?region=us&level=low&fcst=06")
                     .Respond("text/plain", response);

            // Act
            var result = await _windsAloftService.FetchWindsAloftData(6);

            // Assert
            result.ValidTime.Day.Should().Be(22);
            result.ValidTime.Hour.Should().Be(12);
            result.ValidTime.Minute.Should().Be(0);
        }

        [Fact]
        public async Task FetchWindsAloftData_ShouldParseForUseTimesCrossingMidnight()
        {
            // FOR USE 2100-0600Z crosses midnight
            var response = @"000
FBUS31 KWNO 211800
FD1US1
DATA BASED ON 211800Z
VALID 220000Z   FOR USE 2100-0600Z. TEMPS NEG ABV 24000

FT  3000    6000    9000   12000   18000   24000  30000  34000  39000
ABI 1520 2025+10 2530+05 3035-05 3540-15 0045-25 004535 005040 005545";

            _mockHttp.When($"{BaseUrl}?region=us&level=low&fcst=06")
                     .Respond("text/plain", response);

            // Act
            var result = await _windsAloftService.FetchWindsAloftData(6);

            // Assert
            result.ForUseStartTime.Hour.Should().Be(21);
            result.ForUseEndTime.Hour.Should().Be(6);
            // End time should be after start time (next day)
            result.ForUseEndTime.Should().BeAfter(result.ForUseStartTime);
        }

        #endregion

        #region Edge Cases and Error Handling

        [Fact]
        public async Task FetchWindsAloftData_ShouldLogError_WhenExceptionIsThrown()
        {
            // Arrange
            var validFcstHours = 6;
            _mockHttp.When("*").Throw(new HttpRequestException("Network error"));

            // Act
            Func<Task> act = async () => await _windsAloftService.FetchWindsAloftData(validFcstHours);

            // Assert
            await act.Should().ThrowAsync<HttpRequestException>().WithMessage("Network error");

            _logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>()
            );
        }

        [Fact]
        public async Task FetchWindsAloftData_ShouldSkipHeaderLines()
        {
            // Verify header lines like FT, DATA BASED ON, etc. are not parsed as stations
            _mockHttp.When($"{BaseUrl}?region=us&level=low&fcst=06")
                     .Respond("text/plain", MockRawTextResponse);

            // Act
            var result = await _windsAloftService.FetchWindsAloftData(6);

            // Assert - Should not contain any header-like entries
            result.WindTemp.Should().NotContain(s => s.Id == "FT");
            result.WindTemp.Should().NotContain(s => s.Id == "DAT");
            result.WindTemp.Should().NotContain(s => s.Id == "VAL");
            result.WindTemp.Should().NotContain(s => s.Id == "FOR");
            result.WindTemp.Should().NotContain(s => s.Id == "FD1");
            result.WindTemp.Should().NotContain(s => s.Id == "FBU");
        }

        [Fact]
        public async Task FetchWindsAloftData_ShouldHandleMultipleStations()
        {
            var response = @"000
FBUS31 KWNO 211800
FD1US1
DATA BASED ON 211800Z
VALID 220000Z   FOR USE 2000-0300Z. TEMPS NEG ABV 24000

FT  3000    6000    9000   12000   18000   24000  30000  34000  39000
ATL 1520 2025+10 2530+05 3035-05 3540-15 0045-25 004535 005040 005545
BOS 1620 2125+12 2630+07 3135-03 3640-13 0145-23 014533 015038 015543
DEN      2225+08 2730+03 3235-07 3740-17 0245-27 024537 025042 025547
JFK 1720 2225+11 2730+06 3235-04 3740-14 0245-24 024534 025039 025544
LAX 9900 9900+15 2830+08 3335-02 3840-12 0345-22 034532 035037 035542
MIA 1820 2325+14 2830+09 3335-01 3840-11 0445-21 044531 045036 045541
ORD 1920 2425+13 2930+08 3435-02 3940-12 0545-22 054532 055037 055542
SEA 2020 2525+09 3030+04 3535-06 0040-16 0545-26 054536 055041 055546
SFO 2120 2625+11 3130+06 3635-04 0140-14 0645-24 064534 065039 065544";

            _mockHttp.When($"{BaseUrl}?region=us&level=low&fcst=06")
                     .Respond("text/plain", response);

            // Act
            var result = await _windsAloftService.FetchWindsAloftData(6);

            // Assert
            result.WindTemp.Should().HaveCount(9);
            result.WindTemp.Select(s => s.Id).Should().Contain(new[] { "ATL", "BOS", "DEN", "JFK", "LAX", "MIA", "ORD", "SEA", "SFO" });
        }

        [Fact]
        public async Task FetchWindsAloftData_ShouldThrowFormatException_WhenValidTimeNotFound()
        {
            var invalidResponse = @"000
FBUS31 KWNO 211800
FD1US1
DATA BASED ON 211800Z
INVALID FORMAT HERE

FT  3000    6000    9000   12000   18000   24000  30000  34000  39000
ABI 1520 2025+10 2530+05 3035-05 3540-15 0045-25 004535 005040 005545";

            _mockHttp.When($"{BaseUrl}?region=us&level=low&fcst=06")
                     .Respond("text/plain", invalidResponse);

            // Act
            Func<Task> act = async () => await _windsAloftService.FetchWindsAloftData(6);

            // Assert
            await act.Should().ThrowAsync<FormatException>()
                .WithMessage("*Valid time not found*");
        }

        [Fact]
        public async Task FetchWindsAloftData_ShouldThrowFormatException_WhenForUseTimesNotFound()
        {
            var invalidResponse = @"000
FBUS31 KWNO 211800
FD1US1
DATA BASED ON 211800Z
VALID 220000Z   TEMPS NEG ABV 24000

FT  3000    6000    9000   12000   18000   24000  30000  34000  39000
ABI 1520 2025+10 2530+05 3035-05 3540-15 0045-25 004535 005040 005545";

            _mockHttp.When($"{BaseUrl}?region=us&level=low&fcst=06")
                     .Respond("text/plain", invalidResponse);

            // Act
            Func<Task> act = async () => await _windsAloftService.FetchWindsAloftData(6);

            // Assert
            await act.Should().ThrowAsync<FormatException>()
                .WithMessage("*For use times not found*");
        }

        #endregion

        #region Real-World Data Format Tests

        [Fact]
        public async Task FetchWindsAloftData_ShouldParseRealWorldFormatCorrectly()
        {
            // This test uses a format closer to actual FAA winds aloft data
            // Note: When parsing by whitespace, the first token with a temp sign is treated as 6000 ft
            var realWorldResponse = @"000

FBUS31 KWNO 212000

FD1US1

DATA BASED ON 211800Z

VALID 220000Z   FOR USE 2000-0300Z. TEMPS NEG ABV 24000



FT  3000    6000    9000   12000   18000   24000  30000  34000  39000

ABI      0408+08 2909+04 2724+00 3144-16 2951-29 306544 308253 296658
ABQ      2815+05 2820-01 2830-17 2942-29 296143 297753 298359 298565
ABR 3128 3138-20 3148-23 3153-28 3170-37 3083-46 309151 308650 307349
ACK 2128 2444-04 2553-09 2560-14 2675-26 2694-37 761550 762756 763359";

            _mockHttp.When($"{BaseUrl}?region=us&level=low&fcst=06")
                     .Respond("text/plain", realWorldResponse);

            // Act
            var result = await _windsAloftService.FetchWindsAloftData(6);

            // Assert
            result.WindTemp.Should().HaveCount(4);

            // ABI - no 3000 ft, starts at 6000
            var abi = result.WindTemp.First(s => s.Id == "ABI");
            abi.WindTemp.Should().NotContainKey("3000");
            abi.WindTemp["6000"].Direction.Should().Be(40);
            abi.WindTemp["6000"].Speed.Should().Be(8);
            abi.WindTemp["6000"].Temperature.Should().Be(8);

            // ABQ - no 3000 ft, starts at 6000 (first token has temp sign)
            var abq = result.WindTemp.First(s => s.Id == "ABQ");
            abq.WindTemp.Should().NotContainKey("3000");
            abq.WindTemp.Should().ContainKey("6000");
            abq.WindTemp["6000"].Direction.Should().Be(280);
            abq.WindTemp["6000"].Speed.Should().Be(15);
            abq.WindTemp["6000"].Temperature.Should().Be(5);

            // ABR - has all altitudes including 3000
            var abr = result.WindTemp.First(s => s.Id == "ABR");
            abr.WindTemp["3000"].Direction.Should().Be(310);
            abr.WindTemp["3000"].Speed.Should().Be(28);
            abr.WindTemp["3000"].Temperature.Should().BeNull();

            // ACK - check high altitude encoding
            var ack = result.WindTemp.First(s => s.Id == "ACK");
            // 30000: "761550" = (76-50)*10 = 260° at 15+100 = 115 kt, -50°C
            ack.WindTemp["30000"].Direction.Should().Be(260);
            ack.WindTemp["30000"].Speed.Should().Be(115);
            ack.WindTemp["30000"].Temperature.Should().Be(-50);
        }

        #endregion
    }
}