using VFR3D.Infrastructure.Dtos.AircraftPerformanceProfiles;
using VFR3D.Infrastructure.Dtos.Navlog;

namespace VFR3D.Infrastructure.Dtos.Flights;

public record FlightDto
{
    public string Id { get; set; } = string.Empty;
    public string Auth0UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DepartureTime { get; set; } = string.Empty;
    public int PlannedCruisingAltitude { get; set; }
    public List<WaypointDto> Waypoints { get; set; } = [];
    public string AircraftPerformanceId { get; set; } = string.Empty;
    public string? AircraftId { get; set; }
    public double TotalRouteDistance { get; set; }
    public double TotalRouteTimeHours { get; set; }
    public double TotalFuelUsed { get; set; }
    public double AverageWindComponent { get; set; }
    public List<NavigationLegDto> Legs { get; set; } = [];
    public List<string> StateCodesAlongRoute { get; set; } = [];
    public List<string> AirspaceGlobalIds { get; set; } = [];
    public List<string> SpecialUseAirspaceGlobalIds { get; set; } = [];
    public List<string> ObstacleOasNumbers { get; set; } = [];
    public AircraftPerformanceProfileDto? AircraftPerformanceProfile { get; set; }
    public string? RelatedFlightId { get; set; }
}