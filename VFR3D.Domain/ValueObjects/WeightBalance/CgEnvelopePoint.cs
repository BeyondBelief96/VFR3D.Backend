namespace VFR3D.Domain.ValueObjects.WeightBalance;

public class CgEnvelopePoint
{
    public double Weight { get; set; }

    /// <summary>
    /// CG arm value (inches or cm). Used when envelope format is Arm.
    /// </summary>
    public double? Arm { get; set; }

    /// <summary>
    /// Moment/1000 value. Used when envelope format is MomentDividedBy1000.
    /// </summary>
    public double? MomentDividedBy1000 { get; set; }

    /// <summary>
    /// Gets the horizontal axis value based on which field is populated.
    /// </summary>
    public double HorizontalValue => Arm ?? MomentDividedBy1000 ?? 0;
}
