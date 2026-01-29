namespace VFR3D.Infrastructure.Settings;

public class NmsSettings
{
    /// <summary>
    /// Base URL for NMS API endpoints (e.g., https://api-staging.cgifederal-aim.com/nmsapi for staging)
    /// Production: https://api-prod.cgifederal-aim.com/nmsapi (placeholder - update when known)
    /// </summary>
    public string BaseUrl { get; init; } = "https://api-staging.cgifederal-aim.com/nmsapi";

    /// <summary>
    /// Base URL for OAuth2 token endpoint (root URL without /nmsapi path)
    /// Production: https://api-prod.cgifederal-aim.com (placeholder - update when known)
    /// </summary>
    public string AuthBaseUrl { get; init; } = "https://api-staging.cgifederal-aim.com";

    public string ClientId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;
    public int CacheDurationMinutes { get; init; } = 5;
    public double DefaultRouteCorridorRadiusNm { get; init; } = 25;
    public int RequestTimeoutSeconds { get; init; } = 30;
}
