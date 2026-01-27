using VFR3D.Domain.Enums;

namespace VFR3D.Infrastructure.Dtos.WeightBalance;

public record CreateWeightBalanceProfileRequestDto
{
    public string? AircraftId { get; init; }
    public string ProfileName { get; init; } = string.Empty;
    public string? DatumDescription { get; init; }
    public double EmptyWeight { get; init; }
    public double EmptyWeightArm { get; init; }
    public double? MaxRampWeight { get; init; }
    public double MaxTakeoffWeight { get; init; }
    public double? MaxLandingWeight { get; init; }
    public double? MaxZeroFuelWeight { get; init; }
    public WeightUnits WeightUnits { get; init; } = WeightUnits.Pounds;
    public ArmUnits ArmUnits { get; init; } = ArmUnits.Inches;
    public List<LoadingStationDto> LoadingStations { get; init; } = [];
    public List<CgEnvelopeDto> CgEnvelopes { get; init; } = [];
}
