namespace VFR3D.Infrastructure.Dtos.Navlog;

public record BearingAndDistanceResponseDto
{
    public double TrueCourse { get; set; }
    public double MagneticCourse { get; set; }
    public double Distance { get; set; }
}