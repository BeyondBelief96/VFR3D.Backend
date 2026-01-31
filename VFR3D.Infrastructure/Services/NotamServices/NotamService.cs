using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VFR3D.Infrastructure.Dtos.Notam;
using VFR3D.Infrastructure.Interfaces;
using VFR3D.Infrastructure.Settings;

namespace VFR3D.Infrastructure.Services.NotamServices;

/// <summary>
/// NOTAM service with in-memory caching and route aggregation
/// </summary>
public class NotamService : INotamService
{
    private readonly INmsApiClient _nmsApiClient;
    private readonly IMemoryCache _cache;
    private readonly NmsSettings _settings;
    private readonly ILogger<NotamService> _logger;

    private const string CacheKeyPrefixLocation = "notam:location:";
    private const string CacheKeyPrefixRadius = "notam:radius:";

    public NotamService(
        INmsApiClient nmsApiClient,
        IMemoryCache cache,
        IOptions<NmsSettings> settings,
        ILogger<NotamService> logger)
    {
        _nmsApiClient = nmsApiClient ?? throw new ArgumentNullException(nameof(nmsApiClient));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<NotamResponseDto> GetNotamsForAirportAsync(string icaoCodeOrIdent, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(icaoCodeOrIdent))
        {
            throw new ArgumentException("Airport identifier cannot be null or empty", nameof(icaoCodeOrIdent));
        }

        var normalizedIdent = icaoCodeOrIdent.ToUpperInvariant().Trim();
        var cacheKey = $"{CacheKeyPrefixLocation}{normalizedIdent}";

        _logger.LogInformation("Getting NOTAMs for airport: {Identifier}", normalizedIdent);

        var notams = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(_settings.CacheDurationMinutes);

            _logger.LogDebug("Cache miss for {CacheKey}, fetching from NMS API", cacheKey);
            return await _nmsApiClient.GetNotamsByLocationAsync(normalizedIdent, ct);
        });

        return new NotamResponseDto
        {
            Notams = notams ?? [],
            TotalCount = notams?.Count ?? 0,
            RetrievedAt = DateTime.UtcNow,
            QueryLocation = normalizedIdent
        };
    }

    public async Task<NotamResponseDto> GetNotamsByRadiusAsync(double lat, double lon, double radiusNm, CancellationToken ct = default)
    {
        if (radiusNm <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(radiusNm), "Radius must be greater than 0");
        }

        if (radiusNm > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(radiusNm), "Radius cannot exceed 100 nautical miles");
        }

        // Normalize coordinates for cache key (4 decimal places gives ~11m precision)
        var cacheKey = $"{CacheKeyPrefixRadius}{lat:F4}:{lon:F4}:{radiusNm:F1}";

        _logger.LogInformation("Getting NOTAMs within {Radius}nm of {Lat}, {Lon}", radiusNm, lat, lon);

        var notams = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(_settings.CacheDurationMinutes);

            _logger.LogDebug("Cache miss for {CacheKey}, fetching from NMS API", cacheKey);
            return await _nmsApiClient.GetNotamsByRadiusAsync(lat, lon, radiusNm, ct);
        });

        return new NotamResponseDto
        {
            Notams = notams ?? [],
            TotalCount = notams?.Count ?? 0,
            RetrievedAt = DateTime.UtcNow,
            QueryLocation = $"{lat:F4},{lon:F4} ({radiusNm}nm)"
        };
    }

    public async Task<NotamResponseDto> GetNotamsForRouteAsync(NotamQueryByRouteRequest request, CancellationToken ct = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        // RoutePoints takes precedence if provided; otherwise fall back to AirportIdentifiers
        if (request.RoutePoints is { Count: > 0 })
        {
            return await GetNotamsForRoutePointsAsync(request, ct);
        }

        // Legacy mode: use AirportIdentifiers
        if (request.AirportIdentifiers == null || request.AirportIdentifiers.Count == 0)
        {
            throw new ArgumentException("At least one airport identifier or route point is required", nameof(request));
        }

        return await GetNotamsForAirportIdentifiersAsync(request, ct);
    }

    private async Task<NotamResponseDto> GetNotamsForRoutePointsAsync(NotamQueryByRouteRequest request, CancellationToken ct)
    {
        ValidateRoutePoints(request.RoutePoints, request.CorridorRadiusNm);

        _logger.LogInformation("Getting NOTAMs for route with {Count} route points", request.RoutePoints.Count);

        var allNotams = new List<NotamDto>();
        var seenIds = new HashSet<string>();
        var routeDescriptions = new List<string>();

        // Fetch NOTAMs for each route point in parallel
        var tasks = request.RoutePoints
            .Select(async (point, index) =>
            {
                try
                {
                    if (point.IsAirport)
                    {
                        var response = await GetNotamsForAirportAsync(point.AirportIdentifier!, ct);
                        return (Index: index, Point: point, Notams: response.Notams, Error: (Exception?)null);
                    }
                    else
                    {
                        var radius = GetWaypointRadius(point, request.CorridorRadiusNm);
                        var response = await GetNotamsByRadiusAsync(point.Latitude!.Value, point.Longitude!.Value, radius, ct);
                        return (Index: index, Point: point, Notams: response.Notams, Error: (Exception?)null);
                    }
                }
                catch (Exception ex)
                {
                    var pointDesc = FormatRoutePointDescription(point);
                    _logger.LogWarning(ex, "Failed to fetch NOTAMs for route point {PointDescription}", pointDesc);
                    return (Index: index, Point: point, Notams: new List<NotamDto>(), Error: ex);
                }
            })
            .ToList();

        var results = await Task.WhenAll(tasks);

        // Process results in original order
        foreach (var result in results.OrderBy(r => r.Index))
        {
            routeDescriptions.Add(FormatRoutePointDescription(result.Point));

            foreach (var notam in result.Notams)
            {
                var notamId = GetNotamUniqueId(notam);
                if (!string.IsNullOrEmpty(notamId) && seenIds.Add(notamId))
                {
                    allNotams.Add(notam);
                }
                else if (string.IsNullOrEmpty(notamId))
                {
                    // If no ID, include it (can't deduplicate)
                    allNotams.Add(notam);
                }
            }
        }

        return new NotamResponseDto
        {
            Notams = allNotams,
            TotalCount = allNotams.Count,
            RetrievedAt = DateTime.UtcNow,
            QueryLocation = string.Join(" -> ", routeDescriptions)
        };
    }

    private async Task<NotamResponseDto> GetNotamsForAirportIdentifiersAsync(NotamQueryByRouteRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Getting NOTAMs for route with {Count} airports", request.AirportIdentifiers.Count);

        var allNotams = new List<NotamDto>();
        var seenIds = new HashSet<string>();

        // Fetch NOTAMs for each airport in parallel
        var tasks = request.AirportIdentifiers
            .Select(async ident =>
            {
                try
                {
                    var response = await GetNotamsForAirportAsync(ident, ct);
                    return (Identifier: ident, Notams: response.Notams, Error: (Exception?)null);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to fetch NOTAMs for airport {Identifier}", ident);
                    return (Identifier: ident, Notams: new List<NotamDto>(), Error: ex);
                }
            })
            .ToList();

        var results = await Task.WhenAll(tasks);

        // Aggregate and deduplicate NOTAMs
        foreach (var result in results)
        {
            foreach (var notam in result.Notams)
            {
                var notamId = GetNotamUniqueId(notam);
                if (!string.IsNullOrEmpty(notamId) && seenIds.Add(notamId))
                {
                    allNotams.Add(notam);
                }
                else if (string.IsNullOrEmpty(notamId))
                {
                    // If no ID, include it (can't deduplicate)
                    allNotams.Add(notam);
                }
            }
        }

        var routeDescription = string.Join(" -> ", request.AirportIdentifiers.Select(i => i.ToUpperInvariant()));

        return new NotamResponseDto
        {
            Notams = allNotams,
            TotalCount = allNotams.Count,
            RetrievedAt = DateTime.UtcNow,
            QueryLocation = routeDescription
        };
    }

    private void ValidateRoutePoints(List<RoutePointDto> routePoints, double? corridorRadiusNm)
    {
        for (var i = 0; i < routePoints.Count; i++)
        {
            var point = routePoints[i];

            if (point.IsAirport)
            {
                // Airport validation - just needs a valid identifier
                if (string.IsNullOrWhiteSpace(point.AirportIdentifier))
                {
                    throw new ArgumentException($"Route point {i + 1}: Airport identifier cannot be empty");
                }
            }
            else
            {
                // Waypoint validation - needs valid lat/lon
                if (!point.Latitude.HasValue || !point.Longitude.HasValue)
                {
                    throw new ArgumentException($"Route point {i + 1}: Waypoints require both latitude and longitude");
                }

                if (point.Latitude.Value < -90 || point.Latitude.Value > 90)
                {
                    throw new ArgumentException($"Route point {i + 1}: Latitude must be between -90 and 90 degrees");
                }

                if (point.Longitude.Value < -180 || point.Longitude.Value > 180)
                {
                    throw new ArgumentException($"Route point {i + 1}: Longitude must be between -180 and 180 degrees");
                }

                // Validate radius if specified on this point
                if (point.RadiusNm.HasValue)
                {
                    if (point.RadiusNm.Value <= 0)
                    {
                        throw new ArgumentException($"Route point {i + 1}: Radius must be greater than 0");
                    }

                    if (point.RadiusNm.Value > 100)
                    {
                        throw new ArgumentException($"Route point {i + 1}: Radius cannot exceed 100 nautical miles");
                    }
                }
            }
        }
    }

    private double GetWaypointRadius(RoutePointDto point, double? corridorRadiusNm)
    {
        // Fallback chain: point.RadiusNm -> request.CorridorRadiusNm -> settings.DefaultRouteCorridorRadiusNm
        return point.RadiusNm ?? corridorRadiusNm ?? _settings.DefaultRouteCorridorRadiusNm;
    }

    private static string FormatRoutePointDescription(RoutePointDto point)
    {
        if (point.IsAirport)
        {
            return point.AirportIdentifier!.ToUpperInvariant();
        }

        // For waypoints, use name if provided, otherwise format coordinates
        if (!string.IsNullOrWhiteSpace(point.Name))
        {
            return point.Name;
        }

        // Format: "30.1234N, 97.5678W"
        var lat = point.Latitude!.Value;
        var lon = point.Longitude!.Value;
        var latDir = lat >= 0 ? "N" : "S";
        var lonDir = lon >= 0 ? "E" : "W";

        return $"{Math.Abs(lat):F4}{latDir}, {Math.Abs(lon):F4}{lonDir}";
    }

    private static string? GetNotamUniqueId(NotamDto notam)
    {
        // Try to get the NMS NOTAM ID from multiple locations
        if (!string.IsNullOrEmpty(notam.Id))
        {
            return notam.Id;
        }

        return notam.Properties?.CoreNotamData?.Notam?.Id;
    }
}
