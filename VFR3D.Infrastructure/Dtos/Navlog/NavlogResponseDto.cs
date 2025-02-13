namespace VFR3D.Infrastructure.Dtos.Navlog;

public record NavlogResponseDto
{
    public double TotalRouteDistance { get; set; }
    public double TotalRouteTimeHours { get; set; }
    public double TotalFuelUsed { get; set; }
    public double AverageWindComponent { get; set; }
    public List<NavigationLegDto> Legs { get; set; } = [];
}