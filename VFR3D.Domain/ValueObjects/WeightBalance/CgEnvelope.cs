namespace VFR3D.Domain.ValueObjects.WeightBalance;

public class CgEnvelope
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<CgEnvelopePoint> Limits { get; set; } = [];
}
