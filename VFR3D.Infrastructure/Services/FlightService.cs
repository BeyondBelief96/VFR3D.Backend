using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VFR3D.Domain.Entities;
using VFR3D.Domain.Enums;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Dtos.Flights;
using VFR3D.Infrastructure.Dtos.Mappers;
using VFR3D.Infrastructure.Dtos.Navlog;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.Infrastructure.Services;

public class FlightService : IFlightService
{
    private readonly VFR3DDbContext _context;
    private readonly INavlogService _navlogService;
    private readonly ILogger<FlightService> _logger;

    public FlightService(
        VFR3DDbContext context,
        INavlogService navlogService,
        ILogger<FlightService> logger)
    {
        _context = context;
        _navlogService = navlogService;
        _logger = logger;
    }

    public async Task<FlightDto> CreateFlight(string userId, CreateFlightRequestDto request)
    {
        try
        {
            _logger.LogInformation("Creating flight for user {UserId}", userId);

            var navlogResponse = await _navlogService.CalculateNavlog(new NavlogRequestDto
            {
                TimeOfDeparture = request.DepartureTime,
                Waypoints = request.Waypoints,
                PlannedCruisingAltitude = request.PlannedCruisingAltitude,
                AircraftPerformanceProfileId = request.AircraftPerformanceProfileId
            });

            var stateCodesAlongRoute = await GetStateCodesAlongRoute(request.Waypoints);

            var flight = FlightMapper.CreateFromRequest(userId, request);

            // Auto-populate AircraftId from PerformanceProfile if not specified
            if (string.IsNullOrEmpty(flight.AircraftId))
            {
                var performanceProfile = await _context.AircraftPerformanceProfiles
                    .FirstOrDefaultAsync(p => p.Id == request.AircraftPerformanceProfileId && p.UserId == userId);

                if (performanceProfile?.AircraftId != null)
                {
                    flight.AircraftId = performanceProfile.AircraftId;
                }
            }
        
            // Add navigation calculation results
            flight.TotalRouteDistance = navlogResponse.TotalRouteDistance;
            flight.TotalRouteTimeHours = navlogResponse.TotalRouteTimeHours;
            flight.TotalFuelUsed = navlogResponse.TotalFuelUsed;
            flight.AverageWindComponent = navlogResponse.AverageWindComponent;
            flight.StateCodesAlongRoute = stateCodesAlongRoute;
            flight.Legs = navlogResponse.Legs.Select(NavlogLegMapper.MapToEntity).ToList();

            // Persist airspace IDs and relations
            flight.AirspaceGlobalIds = navlogResponse.AirspaceGlobalIds?.ToList() ?? new List<string>();
            flight.SpecialUseAirspaceGlobalIds = navlogResponse.SpecialUseAirspaceGlobalIds?.ToList() ?? new List<string>();
            flight.ObstacleOasNumbers = navlogResponse.ObstacleOasNumbers?.ToList() ?? new List<string>();

            if (flight.AirspaceGlobalIds.Count > 0)
            {
                var relatedAirspaces = await _context.Airspaces
                    .Where(a => flight.AirspaceGlobalIds.Contains(a.GlobalId))
                    .ToListAsync();
                flight.Airspaces = relatedAirspaces;
            }

            if (flight.SpecialUseAirspaceGlobalIds.Count > 0)
            {
                var relatedSuas = await _context.SpecialUseAirspaces
                    .Where(s => flight.SpecialUseAirspaceGlobalIds.Contains(s.GlobalId))
                    .ToListAsync();
                flight.SpecialUseAirspaces = relatedSuas;
            }

            if (flight.ObstacleOasNumbers.Count > 0)
            {
                var relatedObstacles = await _context.Obstacles
                    .Where(o => flight.ObstacleOasNumbers.Contains(o.OasNumber))
                    .ToListAsync();
                flight.Obstacles = relatedObstacles;
            }

            _context.Flights.Add(flight);
            await _context.SaveChangesAsync();

            return FlightMapper.MapToDto(flight);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating flight for user {UserId}", userId);
            throw;
        }
    }

    public async Task<FlightDto> UpdateFlight(string userId, string flightId, UpdateFlightRequestDto request)
    {
        try
        {
            var flight = await _context.Flights
                .Include(f => f.AircraftPerformanceProfile)
                .Include(f => f.Aircraft)
                .Include(f => f.Airspaces)
                .Include(f => f.SpecialUseAirspaces)
                .Include(f => f.Obstacles)
                .FirstOrDefaultAsync(f => f.Id == flightId && f.Auth0UserId == userId);

            if (flight == null)
            {
                throw new KeyNotFoundException($"Flight not found with ID {flightId}");
            }

            // Handle aircraft and performance profile changes with validation
            if (request.AircraftId != null)
            {
                // Aircraft is being changed
                var newAircraft = await _context.Aircraft
                    .Include(a => a.PerformanceProfiles)
                    .FirstOrDefaultAsync(a => a.Id == request.AircraftId && a.UserId == userId);

                if (newAircraft == null)
                {
                    throw new KeyNotFoundException($"Aircraft not found with ID {request.AircraftId}");
                }

                if (request.AircraftPerformanceProfileId != null)
                {
                    // Validate the specified profile belongs to the new aircraft
                    var profileBelongsToAircraft = newAircraft.PerformanceProfiles
                        .Any(p => p.Id == request.AircraftPerformanceProfileId);

                    if (!profileBelongsToAircraft)
                    {
                        throw new InvalidOperationException(
                            $"Performance profile {request.AircraftPerformanceProfileId} does not belong to aircraft {request.AircraftId}");
                    }
                }
                else
                {
                    // No profile specified - use the first available profile from the new aircraft
                    var defaultProfile = newAircraft.PerformanceProfiles.FirstOrDefault();
                    if (defaultProfile == null)
                    {
                        throw new InvalidOperationException(
                            $"Aircraft {request.AircraftId} has no performance profiles. Please create a performance profile first.");
                    }
                    request.AircraftPerformanceProfileId = defaultProfile.Id;
                }
            }
            else if (request.AircraftPerformanceProfileId != null)
            {
                // Only performance profile is being changed - validate it belongs to the current aircraft
                var performanceProfile = await _context.AircraftPerformanceProfiles
                    .FirstOrDefaultAsync(p => p.Id == request.AircraftPerformanceProfileId && p.UserId == userId);

                if (performanceProfile == null)
                {
                    throw new KeyNotFoundException($"Performance profile not found with ID {request.AircraftPerformanceProfileId}");
                }

                // If flight has an aircraft, validate profile belongs to it
                if (flight.AircraftId != null && performanceProfile.AircraftId != flight.AircraftId)
                {
                    throw new InvalidOperationException(
                        $"Performance profile {request.AircraftPerformanceProfileId} does not belong to the flight's aircraft {flight.AircraftId}");
                }
            }

            FlightMapper.UpdateFromRequest(flight, request);

            // Always recalculate navlog to ensure it's current
            var navlogResponse = await _navlogService.CalculateNavlog(new NavlogRequestDto
            {
                TimeOfDeparture = flight.DepartureTime,
                Waypoints = flight.Waypoints.Select(WaypointMapper.MapToDto).ToList(),
                PlannedCruisingAltitude = flight.PlannedCruisingAltitude,
                AircraftPerformanceProfileId = flight.AircraftPerformanceId
            });

            flight.TotalRouteDistance = navlogResponse.TotalRouteDistance;
            flight.TotalRouteTimeHours = navlogResponse.TotalRouteTimeHours;
            flight.TotalFuelUsed = navlogResponse.TotalFuelUsed;
            flight.AverageWindComponent = navlogResponse.AverageWindComponent;
            flight.Legs = navlogResponse.Legs.Select(NavlogLegMapper.MapToEntity).ToList();

            // Update airspace IDs and relations
            flight.AirspaceGlobalIds = navlogResponse.AirspaceGlobalIds?.ToList() ?? new List<string>();
            flight.SpecialUseAirspaceGlobalIds = navlogResponse.SpecialUseAirspaceGlobalIds?.ToList() ?? new List<string>();
            flight.ObstacleOasNumbers = navlogResponse.ObstacleOasNumbers?.ToList() ?? new List<string>();

            // Clear existing relations before setting (EF will manage join table diffs)
            flight.Airspaces.Clear();
            flight.SpecialUseAirspaces.Clear();
            flight.Obstacles.Clear();

            if (flight.AirspaceGlobalIds.Count > 0)
            {
                var relatedAirspaces = await _context.Airspaces
                    .Where(a => flight.AirspaceGlobalIds.Contains(a.GlobalId))
                    .ToListAsync();
                foreach (var a in relatedAirspaces) flight.Airspaces.Add(a);
            }

            if (flight.SpecialUseAirspaceGlobalIds.Count > 0)
            {
                var relatedSuas = await _context.SpecialUseAirspaces
                    .Where(s => flight.SpecialUseAirspaceGlobalIds.Contains(s.GlobalId))
                    .ToListAsync();
                foreach (var s in relatedSuas) flight.SpecialUseAirspaces.Add(s);
            }

            if (flight.ObstacleOasNumbers.Count > 0)
            {
                var relatedObstacles = await _context.Obstacles
                    .Where(o => flight.ObstacleOasNumbers.Contains(o.OasNumber))
                    .ToListAsync();
                foreach (var o in relatedObstacles) flight.Obstacles.Add(o);
            }

            if (request.Waypoints != null)
            {
                flight.StateCodesAlongRoute = await GetStateCodesAlongRoute(request.Waypoints);
            }

            await _context.SaveChangesAsync();
            return await GetFlight(userId, flightId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating flight {FlightId} for user {UserId}", flightId, userId);
            throw;
        }
    }

    public async Task<List<FlightDto>> GetFlights(string userId)
    {
        try
        {
            var flights = await _context.Flights
                .Include(f => f.AircraftPerformanceProfile)
                .Include(f => f.Aircraft)
                .Where(f => f.Auth0UserId == userId)
                .ToListAsync();

            return flights.Select(FlightMapper.MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting flights for user {UserId}", userId);
            throw;
        }
    }

    public async Task<FlightDto> GetFlight(string userId, string flightId)
    {
        try
        {
            var flight = await _context.Flights
                .Include(f => f.AircraftPerformanceProfile)
                .Include(f => f.Aircraft)
                .FirstOrDefaultAsync(f => f.Id == flightId && f.Auth0UserId == userId);

            if (flight == null)
            {
                throw new KeyNotFoundException($"Flight not found with ID {flightId}");
            }

            return FlightMapper.MapToDto(flight);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting flight {FlightId} for user {UserId}", flightId, userId);
            throw;
        }
    }

    public async Task DeleteFlight(string userId, string flightId)
    {
        try
        {
            var flight = await _context.Flights
                .FirstOrDefaultAsync(f => f.Id == flightId && f.Auth0UserId == userId);

            if (flight == null)
            {
                throw new KeyNotFoundException($"Flight not found with ID {flightId}");
            }

            _context.Flights.Remove(flight);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting flight {FlightId} for user {UserId}", flightId, userId);
            throw;
        }
    }

    public async Task<FlightDto> RegenerateNavlog(string userId, string flightId)
    {
        try
        {
            var flight = await _context.Flights
                .Include(f => f.AircraftPerformanceProfile)
                .Include(f => f.Aircraft)
                .Include(f => f.Airspaces)
                .Include(f => f.SpecialUseAirspaces)
                .Include(f => f.Obstacles)
                .FirstOrDefaultAsync(f => f.Id == flightId && f.Auth0UserId == userId);

            if (flight == null)
            {
                throw new KeyNotFoundException($"Flight not found with ID {flightId}");
            }

            var navlogRequest = new NavlogRequestDto
            {
                TimeOfDeparture = flight.DepartureTime,
                Waypoints = flight.Waypoints.Select(WaypointMapper.MapToDto).ToList(),
                PlannedCruisingAltitude = flight.PlannedCruisingAltitude,
                AircraftPerformanceProfileId = flight.AircraftPerformanceId
            };

            var updatedNavlogResponse = await _navlogService.CalculateNavlog(navlogRequest);

            flight.TotalRouteDistance = updatedNavlogResponse.TotalRouteDistance;
            flight.TotalRouteTimeHours = updatedNavlogResponse.TotalRouteTimeHours;
            flight.TotalFuelUsed = updatedNavlogResponse.TotalFuelUsed;
            flight.AverageWindComponent = updatedNavlogResponse.AverageWindComponent;
            flight.Legs = updatedNavlogResponse.Legs.Select(NavlogLegMapper.MapToEntity).ToList();

            // Update airspace IDs and relations
            flight.AirspaceGlobalIds = updatedNavlogResponse.AirspaceGlobalIds?.ToList() ?? new List<string>();
            flight.SpecialUseAirspaceGlobalIds = updatedNavlogResponse.SpecialUseAirspaceGlobalIds?.ToList() ?? new List<string>();
            flight.ObstacleOasNumbers = updatedNavlogResponse.ObstacleOasNumbers?.ToList() ?? new List<string>();

            flight.Airspaces.Clear();
            flight.SpecialUseAirspaces.Clear();
            flight.Obstacles.Clear();

            if (flight.AirspaceGlobalIds.Count > 0)
            {
                var relatedAirspaces = await _context.Airspaces
                    .Where(a => flight.AirspaceGlobalIds.Contains(a.GlobalId))
                    .ToListAsync();
                foreach (var a in relatedAirspaces) flight.Airspaces.Add(a);
            }

            if (flight.SpecialUseAirspaceGlobalIds.Count > 0)
            {
                var relatedSuas = await _context.SpecialUseAirspaces
                    .Where(s => flight.SpecialUseAirspaceGlobalIds.Contains(s.GlobalId))
                    .ToListAsync();
                foreach (var s in relatedSuas) flight.SpecialUseAirspaces.Add(s);
            }

            if (flight.ObstacleOasNumbers.Count > 0)
            {
                var relatedObstacles = await _context.Obstacles
                    .Where(o => flight.ObstacleOasNumbers.Contains(o.OasNumber))
                    .ToListAsync();
                foreach (var o in relatedObstacles) flight.Obstacles.Add(o);
            }

            await _context.SaveChangesAsync();

            return FlightMapper.MapToDto(flight);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error regenerating navlog for flight {FlightId} for user {UserId}",
                flightId, userId);
            throw;
        }
    }

    public async Task<(FlightDto Outbound, FlightDto Return)> CreateRoundTripFlight(string userId, CreateRoundTripFlightRequestDto request)
    {
        try
        {
            _logger.LogInformation("Creating round trip flight for user {UserId}", userId);

            // Create outbound flight
            var outboundRequest = new CreateFlightRequestDto
            {
                Name = request.OutboundName,
                DepartureTime = request.DepartureTime,
                PlannedCruisingAltitude = request.PlannedCruisingAltitude,
                Waypoints = request.Waypoints,
                AircraftPerformanceProfileId = request.AircraftPerformanceProfileId
            };

            var outboundFlight = FlightMapper.CreateFromRequest(userId, outboundRequest);

            // Create return flight with reversed waypoints
            var returnRequest = new CreateFlightRequestDto
            {
                Name = request.ReturnName,
                DepartureTime = request.ReturnDepartureTime,
                PlannedCruisingAltitude = request.PlannedCruisingAltitude,
                Waypoints = request.Waypoints.AsEnumerable().Reverse().ToList(),
                AircraftPerformanceProfileId = request.AircraftPerformanceProfileId
            };

            var returnFlight = FlightMapper.CreateFromRequest(userId, returnRequest);

            // Calculate navlogs for both flights
            var outboundNavlogResponse = await _navlogService.CalculateNavlog(new NavlogRequestDto
            {
                TimeOfDeparture = outboundFlight.DepartureTime,
                Waypoints = request.Waypoints,
                PlannedCruisingAltitude = request.PlannedCruisingAltitude,
                AircraftPerformanceProfileId = request.AircraftPerformanceProfileId
            });

            var returnNavlogResponse = await _navlogService.CalculateNavlog(new NavlogRequestDto
            {
                TimeOfDeparture = returnFlight.DepartureTime,
                Waypoints = returnRequest.Waypoints,
                PlannedCruisingAltitude = request.PlannedCruisingAltitude,
                AircraftPerformanceProfileId = request.AircraftPerformanceProfileId
            });

            // Get state codes for each route
            var outboundStateCodes = await GetStateCodesAlongRoute(outboundRequest.Waypoints);
            var returnStateCodes = await GetStateCodesAlongRoute(returnRequest.Waypoints);

            // Set navigation data for outbound flight
            outboundFlight.TotalRouteDistance = outboundNavlogResponse.TotalRouteDistance;
            outboundFlight.TotalRouteTimeHours = outboundNavlogResponse.TotalRouteTimeHours;
            outboundFlight.TotalFuelUsed = outboundNavlogResponse.TotalFuelUsed;
            outboundFlight.AverageWindComponent = outboundNavlogResponse.AverageWindComponent;
            outboundFlight.StateCodesAlongRoute = outboundStateCodes;
            outboundFlight.Legs = outboundNavlogResponse.Legs.Select(NavlogLegMapper.MapToEntity).ToList();
            outboundFlight.AirspaceGlobalIds = outboundNavlogResponse.AirspaceGlobalIds?.ToList() ?? [];
            outboundFlight.SpecialUseAirspaceGlobalIds = outboundNavlogResponse.SpecialUseAirspaceGlobalIds?.ToList() ?? [];
            outboundFlight.ObstacleOasNumbers = outboundNavlogResponse.ObstacleOasNumbers?.ToList() ?? [];

            // Set navigation data for return flight
            returnFlight.TotalRouteDistance = returnNavlogResponse.TotalRouteDistance;
            returnFlight.TotalRouteTimeHours = returnNavlogResponse.TotalRouteTimeHours;
            returnFlight.TotalFuelUsed = returnNavlogResponse.TotalFuelUsed;
            returnFlight.AverageWindComponent = returnNavlogResponse.AverageWindComponent;
            returnFlight.StateCodesAlongRoute = returnStateCodes;
            returnFlight.Legs = returnNavlogResponse.Legs.Select(NavlogLegMapper.MapToEntity).ToList();
            returnFlight.AirspaceGlobalIds = returnNavlogResponse.AirspaceGlobalIds?.ToList() ?? [];
            returnFlight.SpecialUseAirspaceGlobalIds = returnNavlogResponse.SpecialUseAirspaceGlobalIds?.ToList() ?? [];
            returnFlight.ObstacleOasNumbers = returnNavlogResponse.ObstacleOasNumbers?.ToList() ?? [];

            // Link the flights to each other
            outboundFlight.RelatedFlightId = returnFlight.Id;
            returnFlight.RelatedFlightId = outboundFlight.Id;

            // Process airspace relationships for both flights
            await ProcessAirspaceRelationships(outboundFlight);
            await ProcessAirspaceRelationships(returnFlight);

            // Save both flights
            _context.Flights.Add(outboundFlight);
            _context.Flights.Add(returnFlight);
            await _context.SaveChangesAsync();

            return (FlightMapper.MapToDto(outboundFlight), FlightMapper.MapToDto(returnFlight));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating round trip flight for user {UserId}", userId);
            throw;
        }
    }

    // Helper method to reduce code duplication
    private async Task ProcessAirspaceRelationships(Flight flight)
    {
        if (flight.AirspaceGlobalIds?.Count > 0)
        {
            var relatedAirspaces = await _context.Airspaces
                .Where(a => flight.AirspaceGlobalIds.Contains(a.GlobalId))
                .ToListAsync();
            flight.Airspaces = relatedAirspaces;
        }

        if (flight.SpecialUseAirspaceGlobalIds?.Count > 0)
        {
            var relatedSuas = await _context.SpecialUseAirspaces
                .Where(s => flight.SpecialUseAirspaceGlobalIds.Contains(s.GlobalId))
                .ToListAsync();
            flight.SpecialUseAirspaces = relatedSuas;
        }

        if (flight.ObstacleOasNumbers?.Count > 0)
        {
            var relatedObstacles = await _context.Obstacles
                .Where(o => flight.ObstacleOasNumbers.Contains(o.OasNumber))
                .ToListAsync();
            flight.Obstacles = relatedObstacles;
        }
    }

    private async Task<List<string>> GetStateCodesAlongRoute(List<WaypointDto> waypoints)
    {
        var stateCodes = new HashSet<string>();

        foreach (var waypoint in waypoints)
        {
            if (waypoint.WaypointType == WaypointType.Airport)
            {
                var airport = await _context.Airports
                    .FirstOrDefaultAsync(a => 
                        a.IcaoId == waypoint.Name || 
                        a.ArptId == waypoint.Name);

                if (airport?.StateCode != null)
                {
                    stateCodes.Add(airport.StateCode);
                }
            }
        }

        return stateCodes.ToList();
    }
}