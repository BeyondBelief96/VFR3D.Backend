namespace VFR3D.Infrastructure.Dtos.WeightBalance;

public record WeightBalanceCgResultDto
{
    public double TotalWeight { get; init; }
    public double TotalMoment { get; init; }
    public double CgArm { get; init; }
    public bool IsWithinEnvelope { get; init; }
}
