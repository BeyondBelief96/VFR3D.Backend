using VFR3D.Domain.Entities;
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
            DepartureTime = flight.DepartureTime.ToString("o"),
            PlannedCruisingAltitude = flight.PlannedCruisingAltitude,
            Waypoints = flight.Waypoints.Select(WaypointMapper.MapToDto).ToList(),
            AircraftPerformanceId = flight.AircraftPerformanceId,
            AircraftId = flight.AircraftId,
            TotalRouteDistance = flight.TotalRouteDistance,
            TotalRouteTimeHours = flight.TotalRouteTimeHours,
            TotalFuelUsed = flight.TotalFuelUsed,
            AverageWindComponent = flight.AverageWindComponent,
            Legs = flight.Legs.Select(NavlogLegMapper.MapToDto).ToList(),
            StateCodesAlongRoute = flight.StateCodesAlongRoute,
            AirspaceGlobalIds = flight.AirspaceGlobalIds ?? new List<string>(),
            SpecialUseAirspaceGlobalIds = flight.SpecialUseAirspaceGlobalIds ?? new List<string>(),
            ObstacleOasNumbers = flight.ObstacleOasNumbers ?? new List<string>(),
            AircraftPerformanceProfile = flight.AircraftPerformanceProfile != null
                ? AircraftPerformanceProfileMapper.MapToDto(flight.AircraftPerformanceProfile)
                : null,
            Aircraft = flight.Aircraft != null
                ? AircraftMapper.MapToDto(flight.Aircraft, includeProfiles: false)
                : null,
            RelatedFlightId = flight.RelatedFlightId
        };
    }

    public static Flight MapToEntity(FlightDto dto)
    {
        return new Flight
        {
            Id = dto.Id,
            Auth0UserId = dto.Auth0UserId,
            Name = dto.Name,
            DepartureTime = DateTime.Parse(dto.DepartureTime),
            PlannedCruisingAltitude = dto.PlannedCruisingAltitude,
            Waypoints = dto.Waypoints.Select(WaypointMapper.MapToEntity).ToList(),
            AircraftPerformanceId = dto.AircraftPerformanceId,
            TotalRouteDistance = dto.TotalRouteDistance,
            TotalRouteTimeHours = dto.TotalRouteTimeHours,
            TotalFuelUsed = dto.TotalFuelUsed,
            AverageWindComponent = dto.AverageWindComponent,
            Legs = dto.Legs.Select(NavlogLegMapper.MapToEntity).ToList(),
            StateCodesAlongRoute = dto.StateCodesAlongRoute,
            AirspaceGlobalIds = dto.AirspaceGlobalIds ?? new List<string>(),
            SpecialUseAirspaceGlobalIds = dto.SpecialUseAirspaceGlobalIds ?? new List<string>(),
            ObstacleOasNumbers = dto.ObstacleOasNumbers ?? new List<string>(),
            RelatedFlightId = dto.RelatedFlightId,
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
            AircraftId = request.AircraftId,
            Waypoints = request.Waypoints.Select(WaypointMapper.MapToEntity).ToList(),
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

        if (request.AircraftId != null)
            flight.AircraftId = request.AircraftId;

        if (request.Waypoints != null)
            flight.Waypoints = request.Waypoints.Select(WaypointMapper.MapToEntity).ToList();
    }
}