using VFR3D.Domain.Enums;

namespace VFR3D.Infrastructure.Dtos.Navlog;

public record WaypointDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public double Altitude { get; init; }
    public WaypointType? WaypointType { get; init; }
    public double? RefuelGallons { get; init; }
    public bool? RefuelToFull { get; init; }
    public bool? IsRefuelingStop { get; init; }
}