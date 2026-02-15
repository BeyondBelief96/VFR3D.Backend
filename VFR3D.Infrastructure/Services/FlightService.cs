using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VFR3D.Domain.Entities;
using VFR3D.Domain.Exceptions;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Dtos.Flights;
using VFR3D.Infrastructure.Dtos.Mappers;
using VFR3D.Infrastructure.Dtos.Navlog;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.Infrastructure.Services;

public class FlightService : IFlightService
{
    private readonly VFR3DDbContext _context;
    private readonly IPreflightApiClient _preflightApiClient;
    private readonly ILogger<FlightService> _logger;

    public FlightService(
        VFR3DDbContext context,
        IPreflightApiClient preflightApiClient,
        ILogger<FlightService> logger)
    {
        _context = context;
        _preflightApiClient = preflightApiClient;
        _logger = logger;
    }

    public async Task<FlightDto> CreateFlight(string userId, CreateFlightRequestDto request)
    {
        try
        {
            _logger.LogInformation("Creating flight for user {UserId}", userId);

            var performanceProfile = await _context.AircraftPerformanceProfiles
                .FirstOrDefaultAsync(p => p.Id == request.AircraftPerformanceProfileId && p.UserId == userId);

            if (performanceProfile == null)
            {
                throw new PerformanceProfileNotFoundException(request.AircraftPerformanceProfileId);
            }

            var navlogRequest = new NavlogRequestDto
            {
                TimeOfDeparture = request.DepartureTime,
                Waypoints = request.Waypoints,
                PlannedCruisingAltitude = request.PlannedCruisingAltitude,
                AircraftPerformanceProfileId = request.AircraftPerformanceProfileId
            };

            var navlogResponse = await _preflightApiClient.CalculateNavlogAsync(
                navlogRequest,
                MapToPerformanceData(performanceProfile));

            var flight = FlightMapper.CreateFromRequest(userId, request);

            // Auto-populate AircraftId from PerformanceProfile if not specified
            if (string.IsNullOrEmpty(flight.AircraftId) && performanceProfile.AircraftId != null)
            {
                flight.AircraftId = performanceProfile.AircraftId;
            }

            // Add navigation calculation results
            flight.TotalRouteDistance = navlogResponse.TotalRouteDistance;
            flight.TotalRouteTimeHours = navlogResponse.TotalRouteTimeHours;
            flight.TotalFuelUsed = navlogResponse.TotalFuelUsed;
            flight.AverageWindComponent = navlogResponse.AverageWindComponent;
            flight.StateCodesAlongRoute = new List<string>();
            flight.Legs = navlogResponse.Legs.Select(NavlogLegMapper.MapToEntity).ToList();

            // Persist airspace IDs (string lists only, no FK relationships)
            flight.AirspaceGlobalIds = navlogResponse.AirspaceGlobalIds?.ToList() ?? new List<string>();
            flight.SpecialUseAirspaceGlobalIds = navlogResponse.SpecialUseAirspaceGlobalIds?.ToList() ?? new List<string>();
            flight.ObstacleOasNumbers = navlogResponse.ObstacleOasNumbers?.ToList() ?? new List<string>();

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
                .FirstOrDefaultAsync(f => f.Id == flightId && f.Auth0UserId == userId);

            if (flight == null)
            {
                throw new FlightNotFoundException(userId, flightId);
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
                    throw new AircraftNotFoundException(userId, request.AircraftId);
                }

                if (request.AircraftPerformanceProfileId != null)
                {
                    // Validate the specified profile belongs to the new aircraft
                    var profileBelongsToAircraft = newAircraft.PerformanceProfiles
                        .Any(p => p.Id == request.AircraftPerformanceProfileId);

                    if (!profileBelongsToAircraft)
                    {
                        throw new ValidationException("AircraftPerformanceProfileId",
                            $"Performance profile {request.AircraftPerformanceProfileId} does not belong to aircraft {request.AircraftId}");
                    }
                }
                else
                {
                    // No profile specified - use the first available profile from the new aircraft
                    var defaultProfile = newAircraft.PerformanceProfiles.FirstOrDefault();
                    if (defaultProfile == null)
                    {
                        throw new ValidationException("AircraftId",
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
                    throw new PerformanceProfileNotFoundException(request.AircraftPerformanceProfileId);
                }

                // If flight has an aircraft, validate profile belongs to it
                if (flight.AircraftId != null && performanceProfile.AircraftId != flight.AircraftId)
                {
                    throw new ValidationException("AircraftPerformanceProfileId",
                        $"Performance profile {request.AircraftPerformanceProfileId} does not belong to the flight's aircraft {flight.AircraftId}");
                }
            }

            FlightMapper.UpdateFromRequest(flight, request);

            // Load the performance profile for navlog calculation
            var profile = await _context.AircraftPerformanceProfiles
                .FirstOrDefaultAsync(p => p.Id == flight.AircraftPerformanceId && p.UserId == userId);

            if (profile == null)
            {
                throw new PerformanceProfileNotFoundException(flight.AircraftPerformanceId);
            }

            // Always recalculate navlog to ensure it's current
            var navlogResponse = await _preflightApiClient.CalculateNavlogAsync(
                new NavlogRequestDto
                {
                    TimeOfDeparture = flight.DepartureTime,
                    Waypoints = flight.Waypoints.Select(WaypointMapper.MapToDto).ToList(),
                    PlannedCruisingAltitude = flight.PlannedCruisingAltitude,
                    AircraftPerformanceProfileId = flight.AircraftPerformanceId
                },
                MapToPerformanceData(profile));

            flight.TotalRouteDistance = navlogResponse.TotalRouteDistance;
            flight.TotalRouteTimeHours = navlogResponse.TotalRouteTimeHours;
            flight.TotalFuelUsed = navlogResponse.TotalFuelUsed;
            flight.AverageWindComponent = navlogResponse.AverageWindComponent;
            flight.Legs = navlogResponse.Legs.Select(NavlogLegMapper.MapToEntity).ToList();

            // Update airspace IDs
            flight.AirspaceGlobalIds = navlogResponse.AirspaceGlobalIds?.ToList() ?? new List<string>();
            flight.SpecialUseAirspaceGlobalIds = navlogResponse.SpecialUseAirspaceGlobalIds?.ToList() ?? new List<string>();
            flight.ObstacleOasNumbers = navlogResponse.ObstacleOasNumbers?.ToList() ?? new List<string>();

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
                throw new FlightNotFoundException(userId, flightId);
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
                throw new FlightNotFoundException(userId, flightId);
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
                .FirstOrDefaultAsync(f => f.Id == flightId && f.Auth0UserId == userId);

            if (flight == null)
            {
                throw new FlightNotFoundException(userId, flightId);
            }

            var profile = await _context.AircraftPerformanceProfiles
                .FirstOrDefaultAsync(p => p.Id == flight.AircraftPerformanceId);

            if (profile == null)
            {
                throw new PerformanceProfileNotFoundException(flight.AircraftPerformanceId);
            }

            var updatedNavlogResponse = await _preflightApiClient.CalculateNavlogAsync(
                new NavlogRequestDto
                {
                    TimeOfDeparture = flight.DepartureTime,
                    Waypoints = flight.Waypoints.Select(WaypointMapper.MapToDto).ToList(),
                    PlannedCruisingAltitude = flight.PlannedCruisingAltitude,
                    AircraftPerformanceProfileId = flight.AircraftPerformanceId
                },
                MapToPerformanceData(profile));

            flight.TotalRouteDistance = updatedNavlogResponse.TotalRouteDistance;
            flight.TotalRouteTimeHours = updatedNavlogResponse.TotalRouteTimeHours;
            flight.TotalFuelUsed = updatedNavlogResponse.TotalFuelUsed;
            flight.AverageWindComponent = updatedNavlogResponse.AverageWindComponent;
            flight.Legs = updatedNavlogResponse.Legs.Select(NavlogLegMapper.MapToEntity).ToList();

            // Update airspace IDs
            flight.AirspaceGlobalIds = updatedNavlogResponse.AirspaceGlobalIds?.ToList() ?? new List<string>();
            flight.SpecialUseAirspaceGlobalIds = updatedNavlogResponse.SpecialUseAirspaceGlobalIds?.ToList() ?? new List<string>();
            flight.ObstacleOasNumbers = updatedNavlogResponse.ObstacleOasNumbers?.ToList() ?? new List<string>();

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

            var performanceProfile = await _context.AircraftPerformanceProfiles
                .FirstOrDefaultAsync(p => p.Id == request.AircraftPerformanceProfileId && p.UserId == userId);

            if (performanceProfile == null)
            {
                throw new PerformanceProfileNotFoundException(request.AircraftPerformanceProfileId);
            }

            var performanceData = MapToPerformanceData(performanceProfile);

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
            var returnWaypoints = request.Waypoints.AsEnumerable().Reverse().ToList();
            var returnRequest = new CreateFlightRequestDto
            {
                Name = request.ReturnName,
                DepartureTime = request.ReturnDepartureTime,
                PlannedCruisingAltitude = request.PlannedCruisingAltitude,
                Waypoints = returnWaypoints,
                AircraftPerformanceProfileId = request.AircraftPerformanceProfileId
            };

            var returnFlight = FlightMapper.CreateFromRequest(userId, returnRequest);

            // Calculate navlogs for both flights
            var outboundNavlogResponse = await _preflightApiClient.CalculateNavlogAsync(
                new NavlogRequestDto
                {
                    TimeOfDeparture = outboundFlight.DepartureTime,
                    Waypoints = request.Waypoints,
                    PlannedCruisingAltitude = request.PlannedCruisingAltitude,
                    AircraftPerformanceProfileId = request.AircraftPerformanceProfileId
                },
                performanceData);

            var returnNavlogResponse = await _preflightApiClient.CalculateNavlogAsync(
                new NavlogRequestDto
                {
                    TimeOfDeparture = returnFlight.DepartureTime,
                    Waypoints = returnWaypoints,
                    PlannedCruisingAltitude = request.PlannedCruisingAltitude,
                    AircraftPerformanceProfileId = request.AircraftPerformanceProfileId
                },
                performanceData);

            // Set navigation data for outbound flight
            outboundFlight.TotalRouteDistance = outboundNavlogResponse.TotalRouteDistance;
            outboundFlight.TotalRouteTimeHours = outboundNavlogResponse.TotalRouteTimeHours;
            outboundFlight.TotalFuelUsed = outboundNavlogResponse.TotalFuelUsed;
            outboundFlight.AverageWindComponent = outboundNavlogResponse.AverageWindComponent;
            outboundFlight.StateCodesAlongRoute = new List<string>();
            outboundFlight.Legs = outboundNavlogResponse.Legs.Select(NavlogLegMapper.MapToEntity).ToList();
            outboundFlight.AirspaceGlobalIds = outboundNavlogResponse.AirspaceGlobalIds?.ToList() ?? [];
            outboundFlight.SpecialUseAirspaceGlobalIds = outboundNavlogResponse.SpecialUseAirspaceGlobalIds?.ToList() ?? [];
            outboundFlight.ObstacleOasNumbers = outboundNavlogResponse.ObstacleOasNumbers?.ToList() ?? [];

            // Set navigation data for return flight
            returnFlight.TotalRouteDistance = returnNavlogResponse.TotalRouteDistance;
            returnFlight.TotalRouteTimeHours = returnNavlogResponse.TotalRouteTimeHours;
            returnFlight.TotalFuelUsed = returnNavlogResponse.TotalFuelUsed;
            returnFlight.AverageWindComponent = returnNavlogResponse.AverageWindComponent;
            returnFlight.StateCodesAlongRoute = new List<string>();
            returnFlight.Legs = returnNavlogResponse.Legs.Select(NavlogLegMapper.MapToEntity).ToList();
            returnFlight.AirspaceGlobalIds = returnNavlogResponse.AirspaceGlobalIds?.ToList() ?? [];
            returnFlight.SpecialUseAirspaceGlobalIds = returnNavlogResponse.SpecialUseAirspaceGlobalIds?.ToList() ?? [];
            returnFlight.ObstacleOasNumbers = returnNavlogResponse.ObstacleOasNumbers?.ToList() ?? [];

            // Link the flights to each other
            outboundFlight.RelatedFlightId = returnFlight.Id;
            returnFlight.RelatedFlightId = outboundFlight.Id;

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

    private static NavlogPerformanceDataDto MapToPerformanceData(AircraftPerformanceProfile profile)
    {
        return new NavlogPerformanceDataDto
        {
            ClimbTrueAirspeed = profile.ClimbTrueAirspeed,
            CruiseTrueAirspeed = profile.CruiseTrueAirspeed,
            DescentTrueAirspeed = profile.DescentTrueAirspeed,
            ClimbFpm = profile.ClimbFpm,
            DescentFpm = profile.DescentFpm,
            ClimbFuelBurn = profile.ClimbFuelBurn,
            CruiseFuelBurn = profile.CruiseFuelBurn,
            DescentFuelBurn = profile.DescentFuelBurn,
            SttFuelGals = profile.SttFuelGals,
            FuelOnBoardGals = profile.FuelOnBoardGals
        };
    }
}
