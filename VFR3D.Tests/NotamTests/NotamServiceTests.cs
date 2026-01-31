using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using VFR3D.Infrastructure.Dtos.Notam;
using VFR3D.Infrastructure.Interfaces;
using VFR3D.Infrastructure.Services.NotamServices;
using VFR3D.Infrastructure.Settings;
using Xunit;

namespace VFR3D.Tests.NotamTests;

public class NotamServiceTests
{
    private readonly INmsApiClient _nmsApiClient;
    private readonly IMemoryCache _cache;
    private readonly IOptions<NmsSettings> _settings;
    private readonly ILogger<NotamService> _logger;
    private readonly NotamService _service;

    public NotamServiceTests()
    {
        _nmsApiClient = Substitute.For<INmsApiClient>();
        _cache = new MemoryCache(new MemoryCacheOptions());
        _settings = Options.Create(new NmsSettings
        {
            CacheDurationMinutes = 5,
            DefaultRouteCorridorRadiusNm = 25
        });
        _logger = Substitute.For<ILogger<NotamService>>();

        _service = new NotamService(_nmsApiClient, _cache, _settings, _logger);
    }

    [Fact]
    public async Task GetNotamsForAirportAsync_ShouldReturnNotams_WhenApiReturnsData()
    {
        // Arrange
        var expectedNotams = CreateSampleNotamList("KDFW");
        _nmsApiClient.GetNotamsByLocationAsync("KDFW", Arg.Any<CancellationToken>())
            .Returns(expectedNotams);

        // Act
        var result = await _service.GetNotamsForAirportAsync("KDFW");

        // Assert
        result.Should().NotBeNull();
        result.Notams.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.QueryLocation.Should().Be("KDFW");
    }

    [Fact]
    public async Task GetNotamsForAirportAsync_ShouldNormalizeIdentifier()
    {
        // Arrange
        _nmsApiClient.GetNotamsByLocationAsync("KDFW", Arg.Any<CancellationToken>())
            .Returns(CreateSampleNotamList("KDFW"));

        // Act
        var result = await _service.GetNotamsForAirportAsync("  kdfw  ");

        // Assert
        result.QueryLocation.Should().Be("KDFW");
        await _nmsApiClient.Received(1).GetNotamsByLocationAsync("KDFW", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetNotamsForAirportAsync_ShouldUseCache_OnSubsequentCalls()
    {
        // Arrange
        var expectedNotams = CreateSampleNotamList("KDFW");
        _nmsApiClient.GetNotamsByLocationAsync("KDFW", Arg.Any<CancellationToken>())
            .Returns(expectedNotams);

        // Act
        await _service.GetNotamsForAirportAsync("KDFW");
        await _service.GetNotamsForAirportAsync("KDFW");
        await _service.GetNotamsForAirportAsync("KDFW");

        // Assert - API should only be called once due to caching
        await _nmsApiClient.Received(1).GetNotamsByLocationAsync("KDFW", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetNotamsForAirportAsync_ShouldThrowArgumentException_WhenIdentifierIsEmpty()
    {
        // Act
        Func<Task> act = async () => await _service.GetNotamsForAirportAsync("");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*cannot be null or empty*");
    }

    [Fact]
    public async Task GetNotamsByRadiusAsync_ShouldReturnNotams_WhenApiReturnsData()
    {
        // Arrange
        var expectedNotams = CreateSampleNotamList("NEARBY");
        _nmsApiClient.GetNotamsByRadiusAsync(32.897, -97.038, 25.0, Arg.Any<CancellationToken>())
            .Returns(expectedNotams);

        // Act
        var result = await _service.GetNotamsByRadiusAsync(32.897, -97.038, 25.0);

        // Assert
        result.Should().NotBeNull();
        result.Notams.Should().HaveCount(2);
        result.QueryLocation.Should().Contain("32.8970");
        result.QueryLocation.Should().Contain("-97.0380");
        result.QueryLocation.Should().Contain("25");
    }

    [Fact]
    public async Task GetNotamsByRadiusAsync_ShouldThrowArgumentOutOfRangeException_WhenRadiusTooLarge()
    {
        // Act
        Func<Task> act = async () => await _service.GetNotamsByRadiusAsync(32.897, -97.038, 150.0);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithMessage("*cannot exceed 100*");
    }

    [Fact]
    public async Task GetNotamsByRadiusAsync_ShouldThrowArgumentOutOfRangeException_WhenRadiusIsZero()
    {
        // Act
        Func<Task> act = async () => await _service.GetNotamsByRadiusAsync(32.897, -97.038, 0);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithMessage("*must be greater than 0*");
    }

    [Fact]
    public async Task GetNotamsForRouteAsync_ShouldAggregateNotams_FromMultipleAirports()
    {
        // Arrange
        _nmsApiClient.GetNotamsByLocationAsync("KDFW", Arg.Any<CancellationToken>())
            .Returns(CreateSampleNotamList("KDFW", "NOTAM-DFW-1", "NOTAM-DFW-2"));
        _nmsApiClient.GetNotamsByLocationAsync("KORD", Arg.Any<CancellationToken>())
            .Returns(CreateSampleNotamList("KORD", "NOTAM-ORD-1", "NOTAM-ORD-2"));

        var request = new NotamQueryByRouteRequest
        {
            AirportIdentifiers = ["KDFW", "KORD"]
        };

        // Act
        var result = await _service.GetNotamsForRouteAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Notams.Should().HaveCount(4);
        result.QueryLocation.Should().Be("KDFW -> KORD");
    }

    [Fact]
    public async Task GetNotamsForRouteAsync_ShouldDeduplicateNotams_ByNotamId()
    {
        // Arrange - Both airports return a common NOTAM
        var dfwNotams = new List<NotamDto>
        {
            CreateNotam("NOTAM-123", "KDFW"),
            CreateNotam("NOTAM-DFW-ONLY", "KDFW")
        };
        var ordNotams = new List<NotamDto>
        {
            CreateNotam("NOTAM-123", "KORD"), // Duplicate
            CreateNotam("NOTAM-ORD-ONLY", "KORD")
        };

        _nmsApiClient.GetNotamsByLocationAsync("KDFW", Arg.Any<CancellationToken>())
            .Returns(dfwNotams);
        _nmsApiClient.GetNotamsByLocationAsync("KORD", Arg.Any<CancellationToken>())
            .Returns(ordNotams);

        var request = new NotamQueryByRouteRequest
        {
            AirportIdentifiers = ["KDFW", "KORD"]
        };

        // Act
        var result = await _service.GetNotamsForRouteAsync(request);

        // Assert
        result.Notams.Should().HaveCount(3); // 4 total - 1 duplicate = 3 unique
    }

    [Fact]
    public async Task GetNotamsForRouteAsync_ShouldContinue_WhenOneAirportFails()
    {
        // Arrange
        _nmsApiClient.GetNotamsByLocationAsync("KDFW", Arg.Any<CancellationToken>())
            .Returns(CreateSampleNotamList("KDFW"));
        _nmsApiClient.GetNotamsByLocationAsync("KORD", Arg.Any<CancellationToken>())
            .Returns<List<NotamDto>>(_ => throw new HttpRequestException("API unavailable"));
        _nmsApiClient.GetNotamsByLocationAsync("KLAX", Arg.Any<CancellationToken>())
            .Returns(CreateSampleNotamList("KLAX"));

        var request = new NotamQueryByRouteRequest
        {
            AirportIdentifiers = ["KDFW", "KORD", "KLAX"]
        };

        // Act
        var result = await _service.GetNotamsForRouteAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Notams.Should().HaveCount(4); // 2 from KDFW + 0 from KORD (failed) + 2 from KLAX
    }

    [Fact]
    public async Task GetNotamsForRouteAsync_ShouldThrowArgumentException_WhenNoAirportsOrRoutePoints()
    {
        // Arrange
        var request = new NotamQueryByRouteRequest
        {
            AirportIdentifiers = [],
            RoutePoints = []
        };

        // Act
        Func<Task> act = async () => await _service.GetNotamsForRouteAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*At least one airport*");
    }

    [Fact]
    public async Task GetNotamsForRouteAsync_ShouldThrowArgumentNullException_WhenRequestIsNull()
    {
        // Act
        Func<Task> act = async () => await _service.GetNotamsForRouteAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #region RoutePoints Tests

    [Fact]
    public async Task GetNotamsForRouteAsync_RoutePoints_ShouldReturnNotams_ForWaypointsOnly()
    {
        // Arrange
        _nmsApiClient.GetNotamsByRadiusAsync(30.4082, -97.8538, 25.0, Arg.Any<CancellationToken>())
            .Returns(CreateSampleNotamList("WPT1", "NOTAM-WPT1-1", "NOTAM-WPT1-2"));
        _nmsApiClient.GetNotamsByRadiusAsync(30.1, -97.6, 25.0, Arg.Any<CancellationToken>())
            .Returns(CreateSampleNotamList("WPT2", "NOTAM-WPT2-1"));

        var request = new NotamQueryByRouteRequest
        {
            RoutePoints =
            [
                new RoutePointDto { Name = "Lake Travis", Latitude = 30.4082, Longitude = -97.8538 },
                new RoutePointDto { Latitude = 30.1, Longitude = -97.6 }
            ],
            CorridorRadiusNm = 25
        };

        // Act
        var result = await _service.GetNotamsForRouteAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Notams.Should().HaveCount(3);
        result.QueryLocation.Should().Be("Lake Travis -> 30.1000N, 97.6000W");
    }

    [Fact]
    public async Task GetNotamsForRouteAsync_RoutePoints_ShouldReturnNotams_ForAirportsOnly()
    {
        // Arrange
        _nmsApiClient.GetNotamsByLocationAsync("KDFW", Arg.Any<CancellationToken>())
            .Returns(CreateSampleNotamList("KDFW", "NOTAM-DFW-1", "NOTAM-DFW-2"));
        _nmsApiClient.GetNotamsByLocationAsync("KAUS", Arg.Any<CancellationToken>())
            .Returns(CreateSampleNotamList("KAUS", "NOTAM-AUS-1"));

        var request = new NotamQueryByRouteRequest
        {
            RoutePoints =
            [
                new RoutePointDto { AirportIdentifier = "KDFW" },
                new RoutePointDto { AirportIdentifier = "KAUS" }
            ]
        };

        // Act
        var result = await _service.GetNotamsForRouteAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Notams.Should().HaveCount(3);
        result.QueryLocation.Should().Be("KDFW -> KAUS");
    }

    [Fact]
    public async Task GetNotamsForRouteAsync_RoutePoints_ShouldReturnNotams_ForInterleavedAirportsAndWaypoints()
    {
        // Arrange
        _nmsApiClient.GetNotamsByLocationAsync("KDFW", Arg.Any<CancellationToken>())
            .Returns(CreateSampleNotamList("KDFW", "NOTAM-DFW-1"));
        _nmsApiClient.GetNotamsByRadiusAsync(30.4082, -97.8538, 25.0, Arg.Any<CancellationToken>())
            .Returns(CreateSampleNotamList("WPT", "NOTAM-WPT-1"));
        _nmsApiClient.GetNotamsByLocationAsync("KAUS", Arg.Any<CancellationToken>())
            .Returns(CreateSampleNotamList("KAUS", "NOTAM-AUS-1"));

        var request = new NotamQueryByRouteRequest
        {
            RoutePoints =
            [
                new RoutePointDto { AirportIdentifier = "KDFW" },
                new RoutePointDto { Name = "Lake Travis", Latitude = 30.4082, Longitude = -97.8538 },
                new RoutePointDto { AirportIdentifier = "KAUS" }
            ],
            CorridorRadiusNm = 25
        };

        // Act
        var result = await _service.GetNotamsForRouteAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Notams.Should().HaveCount(3);
        result.QueryLocation.Should().Be("KDFW -> Lake Travis -> KAUS");
    }

    [Fact]
    public async Task GetNotamsForRouteAsync_RoutePoints_ShouldUsePerWaypointRadius()
    {
        // Arrange
        _nmsApiClient.GetNotamsByRadiusAsync(30.4082, -97.8538, 15.0, Arg.Any<CancellationToken>())
            .Returns(CreateSampleNotamList("WPT", "NOTAM-WPT-1"));

        var request = new NotamQueryByRouteRequest
        {
            RoutePoints =
            [
                new RoutePointDto { Latitude = 30.4082, Longitude = -97.8538, RadiusNm = 15 }
            ],
            CorridorRadiusNm = 25 // Should be ignored for this point
        };

        // Act
        await _service.GetNotamsForRouteAsync(request);

        // Assert - Should use point-specific radius (15) not corridor radius (25)
        await _nmsApiClient.Received(1).GetNotamsByRadiusAsync(30.4082, -97.8538, 15.0, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetNotamsForRouteAsync_RoutePoints_ShouldFallbackToCorridorRadius()
    {
        // Arrange
        _nmsApiClient.GetNotamsByRadiusAsync(30.4082, -97.8538, 30.0, Arg.Any<CancellationToken>())
            .Returns(CreateSampleNotamList("WPT", "NOTAM-WPT-1"));

        var request = new NotamQueryByRouteRequest
        {
            RoutePoints =
            [
                new RoutePointDto { Latitude = 30.4082, Longitude = -97.8538 } // No RadiusNm
            ],
            CorridorRadiusNm = 30
        };

        // Act
        await _service.GetNotamsForRouteAsync(request);

        // Assert - Should use corridor radius (30)
        await _nmsApiClient.Received(1).GetNotamsByRadiusAsync(30.4082, -97.8538, 30.0, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetNotamsForRouteAsync_RoutePoints_ShouldFallbackToSettingsRadius()
    {
        // Arrange
        _nmsApiClient.GetNotamsByRadiusAsync(30.4082, -97.8538, 25.0, Arg.Any<CancellationToken>())
            .Returns(CreateSampleNotamList("WPT", "NOTAM-WPT-1"));

        var request = new NotamQueryByRouteRequest
        {
            RoutePoints =
            [
                new RoutePointDto { Latitude = 30.4082, Longitude = -97.8538 } // No RadiusNm
            ]
            // No CorridorRadiusNm
        };

        // Act
        await _service.GetNotamsForRouteAsync(request);

        // Assert - Should use settings default (25)
        await _nmsApiClient.Received(1).GetNotamsByRadiusAsync(30.4082, -97.8538, 25.0, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetNotamsForRouteAsync_RoutePoints_ShouldFormatUnnamedWaypointCoordinates()
    {
        // Arrange
        _nmsApiClient.GetNotamsByRadiusAsync(30.1234, -97.5678, 25.0, Arg.Any<CancellationToken>())
            .Returns(CreateSampleNotamList("WPT", "NOTAM-WPT-1"));

        var request = new NotamQueryByRouteRequest
        {
            RoutePoints =
            [
                new RoutePointDto { Latitude = 30.1234, Longitude = -97.5678 } // No Name
            ],
            CorridorRadiusNm = 25
        };

        // Act
        var result = await _service.GetNotamsForRouteAsync(request);

        // Assert
        result.QueryLocation.Should().Be("30.1234N, 97.5678W");
    }

    [Fact]
    public async Task GetNotamsForRouteAsync_RoutePoints_ShouldFormatNegativeLatitude()
    {
        // Arrange - Southern hemisphere location
        _nmsApiClient.GetNotamsByRadiusAsync(-33.9465, 18.6017, 25.0, Arg.Any<CancellationToken>())
            .Returns(CreateSampleNotamList("WPT", "NOTAM-WPT-1"));

        var request = new NotamQueryByRouteRequest
        {
            RoutePoints =
            [
                new RoutePointDto { Latitude = -33.9465, Longitude = 18.6017 } // Cape Town area
            ],
            CorridorRadiusNm = 25
        };

        // Act
        var result = await _service.GetNotamsForRouteAsync(request);

        // Assert
        result.QueryLocation.Should().Be("33.9465S, 18.6017E");
    }

    [Fact]
    public async Task GetNotamsForRouteAsync_RoutePoints_ShouldDeduplicateAcrossAirportsAndWaypoints()
    {
        // Arrange - Airport and waypoint return same NOTAM
        _nmsApiClient.GetNotamsByLocationAsync("KDFW", Arg.Any<CancellationToken>())
            .Returns(new List<NotamDto>
            {
                CreateNotam("NOTAM-SHARED", "KDFW"),
                CreateNotam("NOTAM-DFW-ONLY", "KDFW")
            });
        _nmsApiClient.GetNotamsByRadiusAsync(32.897, -97.038, 25.0, Arg.Any<CancellationToken>())
            .Returns(new List<NotamDto>
            {
                CreateNotam("NOTAM-SHARED", "NEARBY"), // Duplicate
                CreateNotam("NOTAM-WPT-ONLY", "NEARBY")
            });

        var request = new NotamQueryByRouteRequest
        {
            RoutePoints =
            [
                new RoutePointDto { AirportIdentifier = "KDFW" },
                new RoutePointDto { Latitude = 32.897, Longitude = -97.038 }
            ],
            CorridorRadiusNm = 25
        };

        // Act
        var result = await _service.GetNotamsForRouteAsync(request);

        // Assert
        result.Notams.Should().HaveCount(3); // 4 total - 1 duplicate = 3 unique
    }

    [Fact]
    public async Task GetNotamsForRouteAsync_RoutePoints_ShouldThrowArgumentException_WhenWaypointMissingLatitude()
    {
        // Arrange
        var request = new NotamQueryByRouteRequest
        {
            RoutePoints =
            [
                new RoutePointDto { Longitude = -97.5678 } // Missing latitude
            ]
        };

        // Act
        Func<Task> act = async () => await _service.GetNotamsForRouteAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Waypoints require both latitude and longitude*");
    }

    [Fact]
    public async Task GetNotamsForRouteAsync_RoutePoints_ShouldThrowArgumentException_WhenLatitudeOutOfRange()
    {
        // Arrange
        var request = new NotamQueryByRouteRequest
        {
            RoutePoints =
            [
                new RoutePointDto { Latitude = 91.0, Longitude = -97.5678 } // Invalid latitude
            ]
        };

        // Act
        Func<Task> act = async () => await _service.GetNotamsForRouteAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Latitude must be between -90 and 90*");
    }

    [Fact]
    public async Task GetNotamsForRouteAsync_RoutePoints_ShouldThrowArgumentException_WhenLongitudeOutOfRange()
    {
        // Arrange
        var request = new NotamQueryByRouteRequest
        {
            RoutePoints =
            [
                new RoutePointDto { Latitude = 30.0, Longitude = -181.0 } // Invalid longitude
            ]
        };

        // Act
        Func<Task> act = async () => await _service.GetNotamsForRouteAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Longitude must be between -180 and 180*");
    }

    [Fact]
    public async Task GetNotamsForRouteAsync_RoutePoints_ShouldThrowArgumentException_WhenRadiusTooLarge()
    {
        // Arrange
        var request = new NotamQueryByRouteRequest
        {
            RoutePoints =
            [
                new RoutePointDto { Latitude = 30.0, Longitude = -97.0, RadiusNm = 150 } // Too large
            ]
        };

        // Act
        Func<Task> act = async () => await _service.GetNotamsForRouteAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Radius cannot exceed 100*");
    }

    [Fact]
    public async Task GetNotamsForRouteAsync_RoutePoints_ShouldThrowArgumentException_WhenRadiusIsZero()
    {
        // Arrange
        var request = new NotamQueryByRouteRequest
        {
            RoutePoints =
            [
                new RoutePointDto { Latitude = 30.0, Longitude = -97.0, RadiusNm = 0 }
            ]
        };

        // Act
        Func<Task> act = async () => await _service.GetNotamsForRouteAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Radius must be greater than 0*");
    }

    [Fact]
    public async Task GetNotamsForRouteAsync_RoutePoints_TakesPrecedence_OverAirportIdentifiers()
    {
        // Arrange
        _nmsApiClient.GetNotamsByLocationAsync("KAUS", Arg.Any<CancellationToken>())
            .Returns(CreateSampleNotamList("KAUS", "NOTAM-AUS-1"));

        var request = new NotamQueryByRouteRequest
        {
            AirportIdentifiers = ["KDFW"], // Should be ignored
            RoutePoints =
            [
                new RoutePointDto { AirportIdentifier = "KAUS" } // Should be used
            ]
        };

        // Act
        var result = await _service.GetNotamsForRouteAsync(request);

        // Assert
        result.QueryLocation.Should().Be("KAUS");
        await _nmsApiClient.Received(1).GetNotamsByLocationAsync("KAUS", Arg.Any<CancellationToken>());
        await _nmsApiClient.DidNotReceive().GetNotamsByLocationAsync("KDFW", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetNotamsForRouteAsync_RoutePoints_ShouldContinue_WhenOnePointFails()
    {
        // Arrange
        _nmsApiClient.GetNotamsByLocationAsync("KDFW", Arg.Any<CancellationToken>())
            .Returns(CreateSampleNotamList("KDFW", "NOTAM-DFW-1"));
        _nmsApiClient.GetNotamsByRadiusAsync(30.0, -97.0, 25.0, Arg.Any<CancellationToken>())
            .Returns<List<NotamDto>>(_ => throw new HttpRequestException("API unavailable"));
        _nmsApiClient.GetNotamsByLocationAsync("KAUS", Arg.Any<CancellationToken>())
            .Returns(CreateSampleNotamList("KAUS", "NOTAM-AUS-1"));

        var request = new NotamQueryByRouteRequest
        {
            RoutePoints =
            [
                new RoutePointDto { AirportIdentifier = "KDFW" },
                new RoutePointDto { Latitude = 30.0, Longitude = -97.0 }, // Will fail
                new RoutePointDto { AirportIdentifier = "KAUS" }
            ],
            CorridorRadiusNm = 25
        };

        // Act
        var result = await _service.GetNotamsForRouteAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Notams.Should().HaveCount(2); // 1 from KDFW + 0 from failed + 1 from KAUS
        result.QueryLocation.Should().Be("KDFW -> 30.0000N, 97.0000W -> KAUS");
    }

    [Fact]
    public async Task GetNotamsForRouteAsync_RoutePoints_PreservesOrderInRouteDescription()
    {
        // Arrange
        _nmsApiClient.GetNotamsByLocationAsync("KDFW", Arg.Any<CancellationToken>())
            .Returns(CreateSampleNotamList("KDFW", "NOTAM-1"));
        _nmsApiClient.GetNotamsByRadiusAsync(30.4082, -97.8538, 25.0, Arg.Any<CancellationToken>())
            .Returns(CreateSampleNotamList("WPT1", "NOTAM-2"));
        _nmsApiClient.GetNotamsByRadiusAsync(30.1, -97.6, 25.0, Arg.Any<CancellationToken>())
            .Returns(CreateSampleNotamList("WPT2", "NOTAM-3"));
        _nmsApiClient.GetNotamsByLocationAsync("KAUS", Arg.Any<CancellationToken>())
            .Returns(CreateSampleNotamList("KAUS", "NOTAM-4"));

        var request = new NotamQueryByRouteRequest
        {
            RoutePoints =
            [
                new RoutePointDto { AirportIdentifier = "KDFW" },
                new RoutePointDto { Name = "Lake Travis", Latitude = 30.4082, Longitude = -97.8538 },
                new RoutePointDto { Latitude = 30.1, Longitude = -97.6 },
                new RoutePointDto { AirportIdentifier = "KAUS" }
            ],
            CorridorRadiusNm = 25
        };

        // Act
        var result = await _service.GetNotamsForRouteAsync(request);

        // Assert
        result.QueryLocation.Should().Be("KDFW -> Lake Travis -> 30.1000N, 97.6000W -> KAUS");
    }

    #endregion

    private static List<NotamDto> CreateSampleNotamList(string location, params string[] ids)
    {
        if (ids.Length == 0)
        {
            ids = [$"NOTAM-{location}-1", $"NOTAM-{location}-2"];
        }

        return ids.Select(id => CreateNotam(id, location)).ToList();
    }

    private static NotamDto CreateNotam(string id, string location)
    {
        return new NotamDto
        {
            Type = "Feature",
            Id = id,
            Geometry = new NotamGeometryDto
            {
                Type = "Point",
                Coordinates = new[] { -97.038, 32.897 }
            },
            Properties = new NotamPropertiesDto
            {
                CoreNotamData = new CoreNotamDataDto
                {
                    Notam = new NotamDetailDto
                    {
                        Id = id,
                        Number = "01/001",
                        Location = location,
                        Text = $"Test NOTAM for {location}"
                    }
                }
            }
        };
    }
}
