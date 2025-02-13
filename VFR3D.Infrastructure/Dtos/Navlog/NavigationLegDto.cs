namespace VFR3D.Infrastructure.Dtos.Navlog;

public record NavigationLegDto
{
    public WaypointDto LegStartPoint { get; set; } = new();
    public WaypointDto LegEndPoint { get; set; } = new();
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