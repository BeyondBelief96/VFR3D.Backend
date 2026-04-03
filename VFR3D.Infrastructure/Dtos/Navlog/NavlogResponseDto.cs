namespace VFR3D.Infrastructure.Dtos.Navlog;

public record NavlogResponseDto
{
    public double TotalRouteDistance { get; set; }
    public double TotalRouteTimeHours { get; set; }
    public double TotalFuelUsed { get; set; }
    public double AverageWindComponent { get; set; }
    public List<NavigationLegDto> Legs { get; set; } = [];
    public IReadOnlyCollection<string> AirspaceGlobalIds { get; set; } = Array.Empty<string>();
    public IReadOnlyCollection<string> SpecialUseAirspaceGlobalIds { get; set; } = Array.Empty<string>();
    public IReadOnlyCollection<string> ObstacleOasNumbers { get; set; } = Array.Empty<string>();
}