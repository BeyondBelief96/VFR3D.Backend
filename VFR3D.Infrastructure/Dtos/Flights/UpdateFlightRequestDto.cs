using VFR3D.Infrastructure.Dtos.Navlog;

namespace VFR3D.Infrastructure.Dtos.Flights;

public record UpdateFlightRequestDto
{
    public string? Name { get; set; }
    public DateTime? DepartureTime { get; set; }
    public int? PlannedCruisingAltitude { get; set; }
    public List<WaypointDto>? Waypoints { get; set; }
    public string? AircraftPerformanceProfileId { get; set; }
    public string? AircraftId { get; set; }
}