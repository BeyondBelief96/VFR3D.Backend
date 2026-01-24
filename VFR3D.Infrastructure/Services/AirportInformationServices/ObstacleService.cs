using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using VFR3D.Domain.Enums;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Dtos;
using VFR3D.Infrastructure.Dtos.Mappers;
using VFR3D.Infrastructure.Dtos.Navlog;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.Infrastructure.Services;

public class ObstacleService : IObstacleService
{
    private readonly VFR3DDbContext _context;
    private readonly ILogger<ObstacleService> _logger;
    private readonly GeometryFactory _geometryFactory;

    // 5 nautical miles in meters for route corridor
    private const double CorridorWidthMeters = 5 * 1852;
    // 5 nautical miles in meters for airport vicinity
    private const double AirportVicinityMeters = 5 * 1852;
    // Altitude margin below cruising altitude for route corridor obstacles
    private const int AltitudeMarginFeet = 2000;
    // Minimum height AGL for airport vicinity obstacles (filters out very short obstacles)
    private const int MinAirportVicinityHeightAgl = 200;

    public ObstacleService(
        VFR3DDbContext context,
        ILogger<ObstacleService> logger)
    {
        _context = context;
        _logger = logger;
        _geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
    }

    public async Task<ObstacleDto?> GetByOasNumber(string oasNumber)
    {
        try
        {
            _logger.LogInformation("Getting obstacle by OAS number: {OasNumber}", oasNumber);

            var obstacle = await _context.Obstacles
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.OasNumber == oasNumber.ToUpper());

            return obstacle != null ? ObstacleMapper.ToDto(obstacle) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting obstacle by OAS number: {OasNumber}", oasNumber);
            throw;
        }
    }

    public async Task<IEnumerable<ObstacleDto>> GetByOasNumbers(IEnumerable<string> oasNumbers)
    {
        try
        {
            var oasNumberList = oasNumbers.Select(o => o.ToUpper()).ToList();

            if (oasNumberList.Count == 0)
            {
                return [];
            }

            _logger.LogInformation("Getting {Count} obstacles by OAS numbers", oasNumberList.Count);

            var obstacles = await _context.Obstacles
                .AsNoTracking()
                .Where(o => oasNumberList.Contains(o.OasNumber))
                .OrderByDescending(o => o.HeightAmsl)
                .ToListAsync();

            return obstacles.Select(ObstacleMapper.ToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting obstacles by OAS numbers");
            throw;
        }
    }

    public async Task<IEnumerable<ObstacleDto>> SearchNearby(
        decimal latitude,
        decimal longitude,
        double radiusNm,
        int? minHeightAgl = null,
        int limit = 100)
    {
        try
        {
            _logger.LogInformation(
                "Searching obstacles near ({Lat}, {Lon}) within {Radius} NM, minHeightAgl: {MinHeight}",
                latitude, longitude, radiusNm, minHeightAgl);

            // Convert nautical miles to meters (1 NM = 1852 meters)
            var radiusMeters = radiusNm * 1852;
            var point = _geometryFactory.CreatePoint(new Coordinate((double)longitude, (double)latitude));

            var query = _context.Obstacles
                .AsNoTracking()
                .Where(o => o.Location != null && o.Location.IsWithinDistance(point, radiusMeters));

            if (minHeightAgl.HasValue)
            {
                query = query.Where(o => o.HeightAgl >= minHeightAgl.Value);
            }

            var obstacles = await query
                .OrderByDescending(o => o.HeightAmsl)
                .Take(limit)
                .ToListAsync();

            return obstacles.Select(ObstacleMapper.ToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching obstacles near ({Lat}, {Lon})", latitude, longitude);
            throw;
        }
    }

    public async Task<IEnumerable<ObstacleDto>> GetByState(
        string stateCode,
        int? minHeightAgl = null,
        int limit = 1000)
    {
        try
        {
            _logger.LogInformation("Getting obstacles for state: {StateCode}, minHeightAgl: {MinHeight}",
                stateCode, minHeightAgl);

            var query = _context.Obstacles
                .AsNoTracking()
                .Where(o => o.StateId == stateCode.ToUpper());

            if (minHeightAgl.HasValue)
            {
                query = query.Where(o => o.HeightAgl >= minHeightAgl.Value);
            }

            var obstacles = await query
                .OrderByDescending(o => o.HeightAmsl)
                .Take(limit)
                .ToListAsync();

            return obstacles.Select(ObstacleMapper.ToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting obstacles for state: {StateCode}", stateCode);
            throw;
        }
    }

    public async Task<IEnumerable<ObstacleDto>> GetByBoundingBox(
        decimal minLat,
        decimal maxLat,
        decimal minLon,
        decimal maxLon,
        int? minHeightAgl = null,
        int limit = 1000)
    {
        try
        {
            _logger.LogInformation(
                "Getting obstacles in bounding box: ({MinLat}, {MinLon}) to ({MaxLat}, {MaxLon})",
                minLat, minLon, maxLat, maxLon);

            // Create a bounding box polygon for spatial query
            var coordinates = new[]
            {
                new Coordinate((double)minLon, (double)minLat),
                new Coordinate((double)maxLon, (double)minLat),
                new Coordinate((double)maxLon, (double)maxLat),
                new Coordinate((double)minLon, (double)maxLat),
                new Coordinate((double)minLon, (double)minLat) // Close the ring
            };
            var boundingBox = _geometryFactory.CreatePolygon(coordinates);

            var query = _context.Obstacles
                .AsNoTracking()
                .Where(o => o.Location != null && boundingBox.Contains(o.Location));

            if (minHeightAgl.HasValue)
            {
                query = query.Where(o => o.HeightAgl >= minHeightAgl.Value);
            }

            var obstacles = await query
                .OrderByDescending(o => o.HeightAmsl)
                .Take(limit)
                .ToListAsync();

            return obstacles.Select(ObstacleMapper.ToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting obstacles in bounding box");
            throw;
        }
    }

    public async Task<IReadOnlyCollection<string>> GetObstacleOasNumbersForRouteAsync(
        IEnumerable<WaypointDto> waypoints,
        int? cruisingAltitude = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var waypointList = waypoints.ToList();
            if (waypointList.Count < 2)
            {
                _logger.LogDebug("Insufficient waypoints to build route corridor");
                return Array.Empty<string>();
            }

            var allOasNumbers = new HashSet<string>();

            // 1. Get obstacles along the route corridor (with altitude filtering)
            var corridorObstacles = await GetCorridorObstaclesAsync(waypointList, cruisingAltitude, cancellationToken);
            foreach (var oas in corridorObstacles)
            {
                allOasNumbers.Add(oas);
            }

            // 2. Get ALL obstacles near airports on the route (no altitude filtering)
            var airportObstacles = await GetAirportVicinityObstaclesAsync(waypointList, cancellationToken);
            foreach (var oas in airportObstacles)
            {
                allOasNumbers.Add(oas);
            }

            _logger.LogInformation(
                "Found {TotalCount} obstacles along flight route ({CorridorCount} in corridor, {AirportCount} near airports)",
                allOasNumbers.Count, corridorObstacles.Count, airportObstacles.Count);

            return allOasNumbers.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting obstacles for route");
            throw;
        }
    }

    private async Task<List<string>> GetCorridorObstaclesAsync(
        List<WaypointDto> waypoints,
        int? cruisingAltitude,
        CancellationToken cancellationToken)
    {
        var route = BuildRouteLineString(waypoints);
        if (route == null || route.IsEmpty)
        {
            _logger.LogDebug("Failed to build route LineString from waypoints");
            return [];
        }

        _logger.LogInformation(
            "Searching for obstacles within {CorridorNm} NM corridor of route with {WaypointCount} waypoints, cruising altitude: {Altitude}",
            5, waypoints.Count, cruisingAltitude);

        var query = _context.Obstacles.AsNoTracking()
            .Where(o => o.Location != null && o.Location.IsWithinDistance(route, CorridorWidthMeters));

        // Include obstacles within 1000 ft below cruising altitude or any above
        if (cruisingAltitude.HasValue)
        {
            var minThreatHeight = cruisingAltitude.Value - AltitudeMarginFeet;
            query = query.Where(o => o.HeightAmsl >= minThreatHeight);
        }

        return await query
            .Select(o => o.OasNumber)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    private async Task<List<string>> GetAirportVicinityObstaclesAsync(
        List<WaypointDto> waypoints,
        CancellationToken cancellationToken)
    {
        // Find airport waypoints on the route
        var airportWaypoints = waypoints
            .Where(w => w.WaypointType == WaypointType.Airport)
            .ToList();

        if (airportWaypoints.Count == 0)
        {
            return [];
        }

        _logger.LogInformation(
            "Searching for obstacles >= {MinHeightAgl} ft AGL within {RadiusNm} NM of {AirportCount} airports on route",
            MinAirportVicinityHeightAgl, 5, airportWaypoints.Count);

        var allAirportOasNumbers = new List<string>();

        foreach (var airport in airportWaypoints)
        {
            var airportPoint = _geometryFactory.CreatePoint(new Coordinate(airport.Longitude, airport.Latitude));

            var nearbyObstacles = await _context.Obstacles.AsNoTracking()
                .Where(o => o.Location != null &&
                            o.Location.IsWithinDistance(airportPoint, AirportVicinityMeters) &&
                            o.HeightAgl >= MinAirportVicinityHeightAgl)
                .Select(o => o.OasNumber)
                .ToListAsync(cancellationToken);

            allAirportOasNumbers.AddRange(nearbyObstacles);
        }

        return allAirportOasNumbers.Distinct().ToList();
    }

    private LineString? BuildRouteLineString(List<WaypointDto> waypoints)
    {
        if (waypoints.Count < 2)
            return null;

        var coordinates = waypoints
            .Select(w => new Coordinate(w.Longitude, w.Latitude))
            .ToArray();

        return _geometryFactory.CreateLineString(coordinates);
    }
}
