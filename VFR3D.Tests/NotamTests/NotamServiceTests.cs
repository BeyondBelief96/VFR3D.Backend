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
    public async Task GetNotamsForRouteAsync_ShouldThrowArgumentException_WhenNoAirports()
    {
        // Arrange
        var request = new NotamQueryByRouteRequest
        {
            AirportIdentifiers = []
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
