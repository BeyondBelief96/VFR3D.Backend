using VFR3D.Domain.Enums;

namespace VFR3D.Domain.ValueObjects.Flights;

/// <summary>
/// A waypoint represents a point of interest in a flight plan, represented by a latitude, longitude, and altitude.
/// </summary>
public class Waypoint
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Altitude { get; set; }
    public WaypointType? WaypointType { get; set; }
    public double? RefuelGallons { get; set; }
    public bool? RefuelToFull { get; set; }
    public bool? IsRefuelingStop { get; set; }
}