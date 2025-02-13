namespace VFR3D.Infrastructure.Dtos.Navlog;

public record BearingAndDistanceRequestDto
{
    public WaypointDto StartPoint { get; set; } = new();
    public WaypointDto EndPoint { get; set; } = new();
}