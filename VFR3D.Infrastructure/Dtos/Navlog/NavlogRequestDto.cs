namespace VFR3D.Infrastructure.Dtos.Navlog;

public record NavlogRequestDto
{
    public List<WaypointDto> Waypoints { get; init; } = [];
    public string AircraftPerformanceProfileId { get; init; } = string.Empty;
    public int PlannedCruisingAltitude { get; init; }
    public DateTime TimeOfDeparture { get; init; }
}