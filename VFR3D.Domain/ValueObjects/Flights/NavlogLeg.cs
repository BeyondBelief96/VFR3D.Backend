namespace VFR3D.Domain.ValueObjects.Flights;

public class NavlogLeg
{
    public Waypoint LegStartPoint { get; set; } = new();
    public Waypoint LegEndPoint { get; set; } = new();
    public decimal TrueCourse { get; set; }
    public decimal MagneticHeading { get; set; }
    public decimal MagneticCourse { get; set; }
    public decimal GroundSpeed { get; set; }
    public decimal LegDistance { get; set; }
    public decimal DistanceRemaining { get; set; }
    public DateTime StartLegTime { get; set; }
    public DateTime EndLegTime { get; set; }
    public decimal LegFuelBurnGals { get; set; }
    public decimal RemainingFuelGals { get; set; }
    public int WindDir { get; set; }
    public int WindSpeed { get; set; }
    public decimal HeadwindComponent { get; set; }
    public int TempC { get; set; }
}