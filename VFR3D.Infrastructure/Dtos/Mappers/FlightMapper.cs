using VFR3D.Domain.Entities;
using VFR3D.Infrastructure.Dtos.AircraftPerformanceProfiles;
using VFR3D.Infrastructure.Dtos.Flights;

namespace VFR3D.Infrastructure.Dtos.Mappers;

public static class FlightMapper
{
    public static FlightDto MapToDto(Flight flight)
    {
        return new FlightDto
        {
            Id = flight.Id,
            Auth0UserId = flight.Auth0UserId,
            Name = flight.Name,
            DepartureTime = flight.DepartureTime,
            PlannedCruisingAltitude = flight.PlannedCruisingAltitude,
            Waypoints = flight.Waypoints.Select(WaypointMapper.MapToDto).ToList(),
            AircraftPerformanceId = flight.AircraftPerformanceId,
            TotalRouteDistance = flight.TotalRouteDistance,
            TotalRouteTimeHours = flight.TotalRouteTimeHours,
            TotalFuelUsed = flight.TotalFuelUsed,
            AverageWindComponent = flight.AverageWindComponent,
            Legs = flight.Legs.Select(NavlogLegMapper.MapToDto).ToList(),
            StateCodesAlongRoute = flight.StateCodesAlongRoute,
            AircraftPerformanceProfile = flight.AircraftPerformanceProfile != null 
                ? AircraftPerformanceProfileMapper.MapToDto(flight.AircraftPerformanceProfile)
                : null
        };
    }

    public static Flight MapToEntity(FlightDto dto)
    {
        return new Flight
        {
            Id = dto.Id,
            Auth0UserId = dto.Auth0UserId,
            Name = dto.Name,
            DepartureTime = dto.DepartureTime,
            PlannedCruisingAltitude = dto.PlannedCruisingAltitude,
            Waypoints = dto.Waypoints.Select(WaypointMapper.MapToEntity).ToList(),
            AircraftPerformanceId = dto.AircraftPerformanceId,
            TotalRouteDistance = dto.TotalRouteDistance,
            TotalRouteTimeHours = dto.TotalRouteTimeHours,
            TotalFuelUsed = dto.TotalFuelUsed,
            AverageWindComponent = dto.AverageWindComponent,
            Legs = dto.Legs.Select(NavlogLegMapper.MapToEntity).ToList(),
            StateCodesAlongRoute = dto.StateCodesAlongRoute
        };
    }

    public static Flight CreateFromRequest(string userId, CreateFlightRequestDto request)
    {
        return new Flight
        {
            Id = Guid.NewGuid().ToString(),
            Auth0UserId = userId,
            Name = request.Name,
            DepartureTime = request.DepartureTime,
            PlannedCruisingAltitude = request.PlannedCruisingAltitude,
            AircraftPerformanceId = request.AircraftPerformanceProfileId,
            Waypoints = request.Waypoints.Select(WaypointMapper.MapToEntity).ToList()
        };
    }

    public static void UpdateFromRequest(Flight flight, UpdateFlightRequestDto request)
    {
        if (request.Name != null)
            flight.Name = request.Name;
            
        if (request.DepartureTime.HasValue)
            flight.DepartureTime = request.DepartureTime.Value;
            
        if (request.PlannedCruisingAltitude.HasValue)
            flight.PlannedCruisingAltitude = request.PlannedCruisingAltitude.Value;
            
        if (request.AircraftPerformanceProfileId != null)
            flight.AircraftPerformanceId = request.AircraftPerformanceProfileId;
            
        if (request.Waypoints != null)
            flight.Waypoints = request.Waypoints.Select(WaypointMapper.MapToEntity).ToList();
    }
}