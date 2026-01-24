using VFR3D.Infrastructure.Dtos;
using VFR3D.Infrastructure.Dtos.Navlog;

namespace VFR3D.Infrastructure.Interfaces;

public interface IObstacleService
{
    Task<ObstacleDto?> GetByOasNumber(string oasNumber);
    Task<IEnumerable<ObstacleDto>> SearchNearby(decimal latitude, decimal longitude, double radiusNm, int? minHeightAgl = null, int limit = 100);
    Task<IEnumerable<ObstacleDto>> GetByState(string stateCode, int? minHeightAgl = null, int limit = 1000);
    Task<IEnumerable<ObstacleDto>> GetByBoundingBox(decimal minLat, decimal maxLat, decimal minLon, decimal maxLon, int? minHeightAgl = null, int limit = 1000);

    /// <summary>
    /// Gets obstacles along a flight route using two methods:
    /// 1. Route corridor: obstacles within 10 NM of route with height >= cruising altitude - 2000 ft
    /// 2. Airport vicinity: ALL obstacles within 10 NM of airports on the route (regardless of height)
    /// </summary>
    Task<IReadOnlyCollection<string>> GetObstacleOasNumbersForRouteAsync(
        IEnumerable<WaypointDto> waypoints,
        int? cruisingAltitude = null,
        CancellationToken cancellationToken = default);
}
