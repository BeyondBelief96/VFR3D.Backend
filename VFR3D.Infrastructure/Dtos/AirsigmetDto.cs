using VFR3D.Domain.ValueObjects.Airsigmets;

namespace VFR3D.Infrastructure.Dtos;

public class AirsigmetDto
{
    public string? RawText { get; set; }
    public string? ValidTimeFrom { get; set; }
    public string? ValidTimeTo { get; set; }
    public AirsigmetAltitude? Altitude { get; set; }
    public int? MovementDirDegrees { get; set; }
    public int? MovementSpeedKt { get; set; }
    public AirsigmetHazard? Hazard { get; set; }
    public string? AirsigmetType { get; set; }
    public List<AirsigmetArea>? Areas { get; set; }
}