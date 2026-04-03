namespace VFR3D.Infrastructure.Dtos.WeightBalance;

public record WeightBalanceCalculationResultDto
{
    public WeightBalanceCgResultDto Takeoff { get; init; } = new();
    public WeightBalanceCgResultDto? Landing { get; init; }
    public List<StationBreakdownDto> StationBreakdown { get; init; } = [];
    public string EnvelopeName { get; init; } = string.Empty;
    public List<CgEnvelopePointDto> EnvelopeLimits { get; init; } = [];
    public List<string> Warnings { get; init; } = [];
}
