using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        
            // Add navigation calculation results
            flight.TotalRouteDistance = navlogResponse.TotalRouteDistance;
            flight.TotalRouteTimeHours = navlogResponse.TotalRouteTimeHours;
            flight.TotalFuelUsed = navlogResponse.TotalFuelUsed;
            flight.AverageWindComponent = navlogResponse.AverageWindComponent;
            flight.StateCodesAlongRoute = stateCodesAlongRoute;
            flight.Legs = navlogResponse.Legs.Select(NavlogLegMapper.MapToEntity).ToList();

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
                .FirstOrDefaultAsync(f => f.Id == flightId && f.Auth0UserId == userId);

            if (flight == null)
            {
                throw new KeyNotFoundException($"Flight not found with ID {flightId}");
            }

            FlightMapper.UpdateFromRequest(flight, request);

            // Recalculate navlog if needed
            if (request.DepartureTime.HasValue || request.PlannedCruisingAltitude.HasValue || 
                request.Waypoints != null || request.AircraftPerformanceProfileId != null)
            {
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

                if (request.Waypoints != null)
                {
                    flight.StateCodesAlongRoute = await GetStateCodesAlongRoute(request.Waypoints);
                }
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
            else if (waypoint.Latitude != 0 && waypoint.Longitude != 0)
            {
                // TODO: Implement state lookup by coordinates if needed
            }
        }

        return stateCodes.ToList();
    }
}