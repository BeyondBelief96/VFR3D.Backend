using VFR3D.Domain.Enums;

namespace VFR3D.Infrastructure.Dtos;

/// <summary>
/// DTO for AIRSIGMET hazard with typed hazard type
/// </summary>
public record AirsigmetHazardDto
{
    /// <summary>
    /// The hazard type
    /// </summary>
    public AirsigmetHazardType? Type { get; init; }

    /// <summary>
    /// The severity: LGT, LT-MOD, MOD (AIRMET), MOD-SEV, SEV (SIGMET)
    /// </summary>
    public string? Severity { get; init; }
}
