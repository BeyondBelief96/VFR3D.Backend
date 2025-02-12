namespace VFR3D.Domain.ValueObjects.Flights;

public class Waypoint
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public decimal Altitude { get; set; }
    public string? WaypointType { get; set; }
}