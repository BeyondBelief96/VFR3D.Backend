using VFR3D.Domain.Enums;

namespace VFR3D.Domain.ValueObjects.WeightBalance;

public class CgEnvelope
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The format of the envelope's horizontal axis.
    /// When Arm: Limits use CG arm values (inches or cm).
    /// When MomentDividedBy1000: Limits use Moment/1000 values.
    /// </summary>
    public CgEnvelopeFormat Format { get; set; } = CgEnvelopeFormat.Arm;

    /// <summary>
    /// Polygon points defining the envelope boundary.
    /// The Arm property represents CG arm or Moment/1000 depending on Format.
    /// </summary>
    public List<CgEnvelopePoint> Limits { get; set; } = [];
}
