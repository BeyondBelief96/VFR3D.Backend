namespace VFR3D.Domain.ValueObjects.Flights;

/// <summary>
/// A navlog leg represents a segment of a flight plan, represented by a start and end waypoint, and computed flight parameters.
/// </summary>
public class NavlogLeg
{
    public Waypoint LegStartPoint { get; set; } = new();
    public Waypoint LegEndPoint { get; set; } = new();
    public double TrueCourse { get; set; }
    public double MagneticHeading { get; set; }
    public double MagneticCourse { get; set; }
    public double GroundSpeed { get; set; }
    public double LegDistance { get; set; }
    public double DistanceRemaining { get; set; }
    public DateTime StartLegTime { get; set; }
    public DateTime EndLegTime { get; set; }
    public double LegFuelBurnGals { get; set; }
    public double RemainingFuelGals { get; set; }
    public int WindDir { get; set; }
    public int WindSpeed { get; set; }
    public double HeadwindComponent { get; set; }
    public float TempC { get; set; }
}