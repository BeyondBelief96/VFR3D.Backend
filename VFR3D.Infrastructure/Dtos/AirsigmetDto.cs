using VFR3D.Domain.ValueObjects.Airsigmets;

namespace VFR3D.Infrastructure.Dtos;

public record AirsigmetDto
{
    public string? RawText { get; init; }
    public string? ValidTimeFrom { get; init; }
    public string? ValidTimeTo { get; init; }
    public AirsigmetAltitude? Altitude { get; init; }
    public int? MovementDirDegrees { get; init; }
    public int? MovementSpeedKt { get; init; }
    public AirsigmetHazard? Hazard { get; init; }
    public string? AirsigmetType { get; init; }
    public List<AirsigmetArea>? Areas { get; init; }
}