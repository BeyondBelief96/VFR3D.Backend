namespace VFR3D.Infrastructure.Settings;

public class NmsSettings
{
    /// <summary>
    /// Base URL for NMS API endpoints
    /// Staging: https://api-staging.cgifederal-aim.com/nmsapi
    /// Production: https://api-nms.aim.faa.gov/nmsapi
    /// </summary>
    public string BaseUrl { get; init; } = "https://api-staging.cgifederal-aim.com/nmsapi";

    /// <summary>
    /// Base URL for OAuth2 token endpoint (root URL - /v1/auth/token is appended in code)
    /// Staging: https://api-staging.cgifederal-aim.com
    /// Production: https://api-nms.aim.faa.gov
    /// </summary>
    public string AuthBaseUrl { get; init; } = "https://api-staging.cgifederal-aim.com";

    /// <summary>
    /// Client ID for NMS API OAuth2 authentication
    /// </summary>
    public string ClientId { get; init; } = string.Empty;

    /// <summary>
    /// Client Secret for NMS API OAuth2 authentication
    /// </summary>
    public string ClientSecret { get; init; } = string.Empty;

    /// <summary>
    /// Cache duration in minutes for NMS API responses
    /// </summary>
    public int CacheDurationMinutes { get; init; } = 5;

    /// <summary>
    /// Default route corridor radius in nautical miles for NMS API requests
    /// </summary>
    public double DefaultRouteCorridorRadiusNm { get; init; } = 25;

    /// <summary>
    /// Request timeout in seconds for NMS API calls
    /// </summary>
    public int RequestTimeoutSeconds { get; init; } = 30;
}
