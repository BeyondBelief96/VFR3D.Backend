using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RichardSzalay.MockHttp;
using VFR3D.Infrastructure.Services.WeatherServices;
using Xunit;

namespace VFR3D.Tests.Services.WeatherServicesTests
{
    public class WindsAloftServiceTests
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<WindsAloftService> _logger;
        private readonly WindsAloftService _windsAloftService;
        private readonly MockHttpMessageHandler _mockHttp;

        private const string baseUrl = "https://aviationweather.gov/api/data/windtemp";
        private const string _mockRawTextResponse = @"000
    FBUS31 KWNO 061958
    FD1US1
    DATA BASED ON 061800Z    
    VALID 070000Z   FOR USE 2000-0300Z. TEMPS NEG ABV 24000

    FT  3000    6000    9000   12000   18000   24000  30000  34000  39000
    ABI      1942+11 2430+11 2538+03 2564-13 2585-22 750237 751843 268947
    ABQ              2438+02 2441-07 2571-18 2488-29 742538 743840 742844
    ABR 0508 3510-05 3017-08 2925-14 2841-26 2768-33 268850 278952 278346";
        private const string _mockJsonResponse = @"{
                ""isecs"":1741284000,
                ""date"":""20250306_18"",
                ""fcst"":""06"",
                ""vsecs"":1741305600,
                ""vdate"":""20250307_00"",
                ""level"":""30"",
                ""sites"":[
                    {""id"":""ABI"",""lat"":32.4,""lon"":-99.7,""dir"":0,""spd"":0},
                    {""id"":""ABQ"",""lat"":35,""lon"":-106.6,""dir"":0,""spd"":0},
                    {""id"":""ABR"",""lat"":45.4,""lon"":-98.4,""dir"":50,""spd"":8}
                ]
            }";

        public WindsAloftServiceTests()
        {
            _httpClientFactory = Substitute.For<IHttpClientFactory>();
            _logger = Substitute.For<ILogger<WindsAloftService>>();
            _mockHttp = new MockHttpMessageHandler();
            var client = _mockHttp.ToHttpClient();
            _httpClientFactory.CreateClient(Arg.Any<string>()).Returns(client);
            _windsAloftService = new WindsAloftService(_httpClientFactory, _logger);
        }

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

        [Fact]
        public async Task FetchWindsAloftData_ShouldReturnWindsAloftDto_WhenFcstHoursIsValid()
        {
            // Arrange
            var validFcstHours = 6;
            _mockHttp.When($"{baseUrl}?region=us&level=low&fcst={validFcstHours:D2}")
                     .Respond("text/plain", _mockRawTextResponse);
            foreach (var level in new[] { 30, 60, 90, 120, 180, 240, 300, 340, 390 })
            {
                _mockHttp.When($"{baseUrl}?region=us&level={level}&fcst={validFcstHours:D2}&format=json")
                         .Respond("application/json", _mockJsonResponse);
            }

            // Act
            var result = await _windsAloftService.FetchWindsAloftData(validFcstHours);

            // Assert
            result.Should().NotBeNull();
            result.ValidTime.Should().Be(new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 7, 0, 0, 0, DateTimeKind.Utc));
            result.ForUseStartTime.Should().Be(new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 6, 20, 0, 0, DateTimeKind.Utc));
            result.ForUseEndTime.Should().Be(new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 7, 3, 0, 0, DateTimeKind.Utc));
            result.WindTemp.Should().NotBeEmpty();
            result.WindTemp[0].Id.Should().Be("ABI");
            result.WindTemp[0].Lat.Should().Be(32.4f);
            result.WindTemp[0].Lon.Should().Be(-99.7f);
            result.WindTemp[0].WindTemp["3000"].Direction.Should().Be(0);
            result.WindTemp[0].WindTemp["3000"].Speed.Should().Be(0);
            result.WindTemp[0].WindTemp["3000"].Temperature.Should().BeNull();
        }

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

            // Fix: Use Received() with Any<HttpRequestException>() instead of specific message check
            _logger.Received(1).AnyLogOfType(LogLevel.Error);
        }
    }
}