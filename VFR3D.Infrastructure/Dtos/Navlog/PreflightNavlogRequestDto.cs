namespace VFR3D.Infrastructure.Dtos.Navlog;

/// <summary>
/// Request DTO matching preflight.api's /api/v1/navlog/calculate endpoint.
/// Uses inline performance data instead of a DB-backed profile ID.
/// </summary>
public record PreflightNavlogRequestDto
{
    public List<WaypointDto> Waypoints { get; init; } = [];
    public int PlannedCruisingAltitude { get; init; }
    public DateTime TimeOfDeparture { get; init; }
    public NavlogPerformanceDataDto PerformanceData { get; init; } = new();
}
