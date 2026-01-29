namespace VFR3D.Domain.ValueObjects.WeightBalance;

/// <summary>
/// Represents the result of a CG calculation (takeoff or landing).
/// </summary>
public class WeightBalanceCgResult
{
    public double TotalWeight { get; set; }
    public double TotalMoment { get; set; }
    public double CgArm { get; set; }
    public bool IsWithinEnvelope { get; set; }
}
