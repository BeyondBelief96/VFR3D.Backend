using VFR3D.Domain.Enums;

namespace VFR3D.Domain.ValueObjects.Flights;

public class Waypoint
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Altitude { get; set; }
    public WaypointType? WaypointType { get; set; }
}