namespace VFR3D.Infrastructure.Dtos.WeightBalance;

public record LoadingStationDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public double Arm { get; init; }
    public double MaxWeight { get; init; }
    public bool IsFuelStation { get; init; }
    public double? FuelCapacityGallons { get; init; }
    public double? FuelWeightPerGallon { get; init; }
}
