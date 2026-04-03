using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using RichardSzalay.MockHttp;
using VFR3D.Infrastructure.Dtos.Notam;
using VFR3D.Infrastructure.Services.NotamServices;
using VFR3D.Infrastructure.Settings;
using Xunit;

namespace VFR3D.Tests.NotamTests;

public class NmsApiClientTests
{
    private readonly MockHttpMessageHandler _mockHttp;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptions<NmsSettings> _settings;
    private readonly ILogger<NmsApiClient> _logger;
    private readonly NmsApiClient _client;

    private const string BaseUrl = "https://api-staging.cgifederal-aim.com/nmsapi";
    private const string AuthBaseUrl = "https://api-staging.cgifederal-aim.com";
    private const string TestClientId = "test-client-id";
    private const string TestClientSecret = "test-client-secret";
    private const string TestAccessToken = "test-access-token-12345";

    public NmsApiClientTests()
    {
        _mockHttp = new MockHttpMessageHandler();
        _httpClientFactory = Substitute.For<IHttpClientFactory>();
        _httpClientFactory.CreateClient("NmsApi").Returns(_ => _mockHttp.ToHttpClient());

        _settings = Options.Create(new NmsSettings
        {
            BaseUrl = BaseUrl,
            AuthBaseUrl = AuthBaseUrl,
            ClientId = TestClientId,
            ClientSecret = TestClientSecret,
            CacheDurationMinutes = 5,
            RequestTimeoutSeconds = 30
        });

        _logger = Substitute.For<ILogger<NmsApiClient>>();

        _client = new NmsApiClient(_httpClientFactory, _settings, _logger);
    }

    [Fact]
    public async Task GetNotamsByLocationAsync_ShouldReturnNotams_WhenApiReturnsValidResponse()
    {
        // Arrange
        SetupTokenEndpoint();
        SetupLocationEndpoint("KDFW", CreateSampleGeoJsonResponse());

        // Act
        var result = await _client.GetNotamsByLocationAsync("KDFW");

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result[0].Type.Should().Be("Feature");
        result[0].Properties?.CoreNotamData?.Notam?.Location.Should().Be("KDFW");
    }

    [Fact]
    public async Task GetNotamsByLocationAsync_ShouldNormalizeLocationToUppercase()
    {
        // Arrange
        SetupTokenEndpoint();
        _mockHttp.When($"{BaseUrl}/v1/notams?location=KDFW")
            .Respond("application/json", CreateSampleGeoJsonResponse());

        // Act
        var result = await _client.GetNotamsByLocationAsync("kdfw");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetNotamsByLocationAsync_ShouldThrowArgumentException_WhenLocationIsEmpty()
    {
        // Act
        Func<Task> act = async () => await _client.GetNotamsByLocationAsync("");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*cannot be null or empty*");
    }

    [Fact]
    public async Task GetNotamsByRadiusAsync_ShouldReturnNotams_WhenApiReturnsValidResponse()
    {
        // Arrange
        SetupTokenEndpoint();
        _mockHttp.When($"{BaseUrl}/v1/notams*")
            .WithQueryString("latitude", "32.897000")
            .Respond("application/json", CreateSampleGeoJsonResponse());

        // Act
        var result = await _client.GetNotamsByRadiusAsync(32.897, -97.038, 25.0);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetNotamsByRadiusAsync_ShouldThrowArgumentOutOfRangeException_WhenRadiusTooLarge()
    {
        // Act
        Func<Task> act = async () => await _client.GetNotamsByRadiusAsync(32.897, -97.038, 150.0);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithMessage("*must be between 0 and 100*");
    }

    [Fact]
    public async Task GetNotamsByRadiusAsync_ShouldThrowArgumentOutOfRangeException_WhenRadiusIsZero()
    {
        // Act
        Func<Task> act = async () => await _client.GetNotamsByRadiusAsync(32.897, -97.038, 0);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task GetNotamsByLocationAsync_ShouldRefreshToken_When401Received()
    {
        // Arrange
        var tokenCallCount = 0;
        _mockHttp.When(HttpMethod.Post, $"{AuthBaseUrl}/v1/auth/token")
            .Respond(_ =>
            {
                tokenCallCount++;
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(CreateTokenResponse(), Encoding.UTF8, "application/json")
                };
            });

        var firstCall = true;
        _mockHttp.When($"{BaseUrl}/v1/notams?location=KDFW")
            .Respond(_ =>
            {
                if (firstCall)
                {
                    firstCall = false;
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized);
                }
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(CreateSampleGeoJsonResponse(), Encoding.UTF8, "application/json")
                };
            });

        // Act
        var result = await _client.GetNotamsByLocationAsync("KDFW");

        // Assert
        result.Should().NotBeNull();
        tokenCallCount.Should().Be(2); // Initial token + refresh after 401
    }

    [Fact]
    public async Task GetNotamsByLocationAsync_ShouldReuseToken_WhenNotExpired()
    {
        // Arrange
        var tokenCallCount = 0;
        _mockHttp.When(HttpMethod.Post, $"{AuthBaseUrl}/v1/auth/token")
            .Respond(_ =>
            {
                tokenCallCount++;
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(CreateTokenResponse(3600), Encoding.UTF8, "application/json")
                };
            });

        _mockHttp.When($"{BaseUrl}/v1/notams*")
            .Respond("application/json", CreateSampleGeoJsonResponse());

        // Act
        await _client.GetNotamsByLocationAsync("KDFW");
        await _client.GetNotamsByLocationAsync("KORD");
        await _client.GetNotamsByLocationAsync("KLAX");

        // Assert
        tokenCallCount.Should().Be(1); // Only one token request
    }

    [Fact]
    public async Task GetNotamsByLocationAsync_ShouldThrow_WhenCredentialsNotConfigured()
    {
        // Arrange
        var settingsNoCredentials = Options.Create(new NmsSettings
        {
            BaseUrl = BaseUrl,
            ClientId = "",
            ClientSecret = ""
        });
        var clientNoCredentials = new NmsApiClient(_httpClientFactory, settingsNoCredentials, _logger);

        // Act
        Func<Task> act = async () => await clientNoCredentials.GetNotamsByLocationAsync("KDFW");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*ClientId and ClientSecret must be configured*");
    }

    [Fact]
    public async Task GetNotamsByLocationAsync_ShouldReturnEmptyList_WhenNoNotamsFound()
    {
        // Arrange
        SetupTokenEndpoint();
        var emptyResponse = """{"status": "Success", "data": {"geojson": []}}""";
        _mockHttp.When($"{BaseUrl}/v1/notams*")
            .WithQueryString("location", "KXYZ")
            .Respond("application/json", emptyResponse);

        // Act
        var result = await _client.GetNotamsByLocationAsync("KXYZ");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    private void SetupTokenEndpoint()
    {
        _mockHttp.When(HttpMethod.Post, $"{AuthBaseUrl}/v1/auth/token")
            .Respond("application/json", CreateTokenResponse());
    }

    private void SetupLocationEndpoint(string location, string response)
    {
        _mockHttp.When($"{BaseUrl}/v1/notams?location={location}")
            .Respond("application/json", response);
    }

    private static string CreateTokenResponse(int expiresIn = 3600)
    {
        return JsonSerializer.Serialize(new
        {
            access_token = TestAccessToken,
            issued_at = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            expires_in = expiresIn
        });
    }

    private static string CreateSampleGeoJsonResponse()
    {
        return """
        {
            "status": "Success",
            "data": {
                "geojson": [
                    {
                        "type": "Feature",
                        "id": "NOTAM-123",
                        "geometry": {
                            "type": "Point",
                            "coordinates": [-97.038, 32.897]
                        },
                        "properties": {
                            "coreNOTAMData": {
                                "notam": {
                                    "id": "NOTAM-123",
                                    "number": "01/001",
                                    "series": "A",
                                    "type": "N",
                                    "issued": "2024-01-15T12:00:00Z",
                                    "effectiveStart": "2024-01-15T12:00:00Z",
                                    "effectiveEnd": "2024-02-15T12:00:00Z",
                                    "location": "KDFW",
                                    "text": "RWY 18L/36R CLSD"
                                },
                                "notamTranslation": [
                                    {
                                        "type": "LOCAL_FORMAT",
                                        "simpleText": "Runway 18L/36R closed"
                                    }
                                ]
                            }
                        }
                    }
                ]
            }
        }
        """;
    }
}
