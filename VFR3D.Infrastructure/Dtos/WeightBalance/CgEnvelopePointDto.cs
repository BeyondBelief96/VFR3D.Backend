namespace VFR3D.Infrastructure.Dtos.WeightBalance;

public record CgEnvelopePointDto
{
    public double Weight { get; init; }

    /// <summary>
    /// CG arm value (inches or cm). Used when envelope format is Arm.
    /// </summary>
    public double? Arm { get; init; }

    /// <summary>
    /// Moment/1000 value. Used when envelope format is MomentDividedBy1000.
    /// </summary>
    public double? MomentDividedBy1000 { get; init; }
}
