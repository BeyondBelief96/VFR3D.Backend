using VFR3D.Infrastructure.Dtos;
using VFR3D.Infrastructure.Dtos.Navlog;

namespace VFR3D.Infrastructure.Interfaces;

public interface IAirspaceService
{
    Task<IEnumerable<AirspaceDto>> GetByClasses(string[] airspaceClasses);
    Task<IEnumerable<AirspaceDto>> GetByCities(string[] cities);
    Task<IEnumerable<AirspaceDto>> GetByStates(string[] states);
    Task<IEnumerable<SpecialUseAirspaceDto>> GetByTypeCodes(string[] typeCodes);
    Task<IEnumerable<AirspaceDto>> GetByIcaoOrIdents(string[] icaoOrIdents);

    // New: spatial queries along a route
    Task<IReadOnlyCollection<string>> GetAirspaceGlobalIdsForRouteAsync(IEnumerable<WaypointDto> waypoints, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<string>> GetSpecialUseAirspaceGlobalIdsForRouteAsync(IEnumerable<WaypointDto> waypoints, CancellationToken cancellationToken = default);
}