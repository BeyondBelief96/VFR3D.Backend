using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Dtos;
using VFR3D.Infrastructure.Dtos.Mappers;
using VFR3D.Infrastructure.Dtos.Navlog;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.Infrastructure.Services.AirportInformationServices
{
    public class AirspaceService : IAirspaceService
    {
        private readonly VFR3DDbContext _context;
        private readonly ILogger<AirspaceService> _logger;
        private readonly GeometryFactory _geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

        public AirspaceService(
            VFR3DDbContext context,
            ILogger<AirspaceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<AirspaceDto>> GetByClasses(string[] airspaceClasses)
        {
            try
            {
                _logger.LogInformation("Getting airspaces by classes: {Classes}", 
                    string.Join(", ", airspaceClasses));

                var upperClasses = airspaceClasses.Select(c => c.ToUpper()).ToArray();
                var airspaces = await _context.Airspaces
                    .Where(a => a.Class != null && upperClasses.Contains(a.Class))
                    .OrderBy(a => a.Name)
                    .ToListAsync();

                return airspaces.Select(AirspaceMapper.ToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting airspaces by classes: {Classes}", 
                    string.Join(", ", airspaceClasses));
                throw;
            }
        }

        public async Task<IEnumerable<AirspaceDto>> GetByCities(string[] cities)
        {
            try
            {
                _logger.LogInformation("Getting airspaces by cities: {Cities}", 
                    string.Join(", ", cities));

                var upperCities = cities.Select(c => c.ToUpper()).ToArray();
                var airspaces = await _context.Airspaces
                    .Where(a => a.City != null && upperCities.Contains(a.City.ToUpper()))
                    .OrderBy(a => a.Name)
                    .ToListAsync();

                return airspaces.Select(AirspaceMapper.ToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting airspaces by cities: {Cities}", 
                    string.Join(", ", cities));
                throw;
            }
        }

        public async Task<IEnumerable<AirspaceDto>> GetByStates(string[] states)
        {
            try
            {
                _logger.LogInformation("Getting airspaces by states: {States}", 
                    string.Join(", ", states));

                var upperStates = states.Select(s => s.ToUpper()).ToArray();
                var airspaces = await _context.Airspaces
                    .Where(a => a.State != null && upperStates.Contains(a.State))
                    .OrderBy(a => a.Name)
                    .ToListAsync();

                return airspaces.Select(AirspaceMapper.ToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting airspaces by states: {States}", 
                    string.Join(", ", states));
                throw;
            }
        }

        public async Task<IEnumerable<SpecialUseAirspaceDto>> GetByTypeCodes(string[] typeCodes)
        {
            try
            {
                _logger.LogInformation("Getting special use airspaces by type codes: {TypeCodes}", 
                    string.Join(", ", typeCodes));

                var upperTypeCodes = typeCodes.Select(t => t.ToUpper()).ToArray();
                var airspaces = await _context.SpecialUseAirspaces
                    .Where(a => a.TypeCode != null && upperTypeCodes.Contains(a.TypeCode))
                    .OrderBy(a => a.Name)
                    .ToListAsync();

                return airspaces.Select(AirspaceMapper.ToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting special use airspaces by type codes: {TypeCodes}", 
                    string.Join(", ", typeCodes));
                throw;
            }
        }

        public async Task<IEnumerable<AirspaceDto>> GetByIcaoOrIdents(string[] icaoOrIdents)
        {
            try
            {
                _logger.LogInformation("Getting airspaces by ICAO codes or idents: {IcaoOrIdents}", 
                    string.Join(", ", icaoOrIdents));

                var upperCodes = icaoOrIdents.Select(i => i.ToUpper()).ToArray();
                var airspaces = await _context.Airspaces
                    .Where(a => 
                        (a.IcaoId != null && upperCodes.Contains(a.IcaoId)) || 
                        (a.Ident != null && upperCodes.Contains(a.Ident)))
                    .OrderBy(a => a.Name)
                    .ToListAsync();

                return airspaces.Select(AirspaceMapper.ToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting airspaces by ICAO codes or idents: {IcaoOrIdents}", 
                    string.Join(", ", icaoOrIdents));
                throw;
            }
        }

        public async Task<IReadOnlyCollection<string>> GetAirspaceGlobalIdsForRouteAsync(
            IEnumerable<WaypointDto> waypoints,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var route = BuildRouteLineString(waypoints);
                if (route == null) return Array.Empty<string>();

                var routeEnvelope = route.Envelope as Polygon;
                if (routeEnvelope == null) return Array.Empty<string>();

                // Prefilter by route bounding box, then intersect the actual line
                var query = _context.Airspaces.AsNoTracking()
                    .Where(a => a.Geometry != null)
                    .Where(a => a.Geometry!.Intersects(routeEnvelope))
                    .Where(a => a.Geometry!.Intersects(route))
                    .Select(a => a.GlobalId!)
                    .Distinct();

                // Include airspaces for airport waypoints explicitly by ICAO/Ident
                var airportCodes = ExtractAirportCodes(waypoints);
                if (airportCodes.Count > 0)
                {
                    var airportAirspaceIds = _context.Airspaces.AsNoTracking()
                        .Where(a => (a.IcaoId != null && airportCodes.Contains(a.IcaoId))
                                    || (a.Ident != null && airportCodes.Contains(a.Ident)))
                        .Select(a => a.GlobalId!);

                    query = query.Union(airportAirspaceIds);
                }

                var ids = await query.ToListAsync(cancellationToken);
                return ids;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting airspace IDs for route");
                throw;
            }
        }

        public async Task<IReadOnlyCollection<string>> GetSpecialUseAirspaceGlobalIdsForRouteAsync(
            IEnumerable<WaypointDto> waypoints,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var route = BuildRouteLineString(waypoints);
                if (route == null) return Array.Empty<string>();

                var routeEnvelope = route.Envelope as Polygon;
                if (routeEnvelope == null) return Array.Empty<string>();

                var ids = await _context.SpecialUseAirspaces.AsNoTracking()
                    .Where(s => s.Geometry != null)
                    .Where(s => s.Geometry!.Intersects(routeEnvelope))
                    .Where(s => s.Geometry!.Intersects(route))
                    .Select(s => s.GlobalId!)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                return ids;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting special use airspace IDs for route");
                throw;
            }
        }

        private LineString? BuildRouteLineString(IEnumerable<WaypointDto> waypoints)
        {
            var coords = waypoints
                .Select(w => new Coordinate(w.Longitude, w.Latitude))
                .ToArray();

            if (coords.Length < 2) return null;
            return _geometryFactory.CreateLineString(coords);
        }

        private static HashSet<string> ExtractAirportCodes(IEnumerable<WaypointDto> waypoints)
        {
            var codes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var w in waypoints)
            {
                if (w.WaypointType?.ToString().Equals("Airport", StringComparison.OrdinalIgnoreCase) == true
                    || (!string.IsNullOrWhiteSpace(w.Name) && w.Name.Length >= 3))
                {
                    var code = w.Name?.Trim();
                    if (string.IsNullOrEmpty(code)) continue;

                    codes.Add(code.ToUpperInvariant());
                    if (code.Length == 4 && code.StartsWith("K", StringComparison.OrdinalIgnoreCase))
                    {
                        codes.Add(code[1..].ToUpperInvariant());
                    }
                }
            }

            return codes;
        }
    }
}