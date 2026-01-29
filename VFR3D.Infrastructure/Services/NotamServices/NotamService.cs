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

        if (request.AirportIdentifiers == null || request.AirportIdentifiers.Count == 0)
        {
            throw new ArgumentException("At least one airport identifier is required", nameof(request));
        }

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
