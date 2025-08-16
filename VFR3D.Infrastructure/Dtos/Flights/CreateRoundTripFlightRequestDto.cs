using VFR3D.Infrastructure.Dtos.Navlog;

namespace VFR3D.Infrastructure.Dtos.Flights;

public record CreateRoundTripFlightRequestDto
{
    public string OutboundName { get; init; } = string.Empty;
    public string ReturnName { get; init; } = string.Empty;
    public DateTime DepartureTime { get; init; } = DateTime.UtcNow;
    public DateTime ReturnDepartureTime { get; init; } = DateTime.UtcNow.AddHours(1);
    public int PlannedCruisingAltitude { get; init; }
    public List<WaypointDto> Waypoints { get; init; } = [];
    public string AircraftPerformanceProfileId { get; init; } = string.Empty;
}