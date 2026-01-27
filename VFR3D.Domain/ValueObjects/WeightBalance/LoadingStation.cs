namespace VFR3D.Domain.ValueObjects.WeightBalance;

public class LoadingStation
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public double Arm { get; set; }
    public double MaxWeight { get; set; }
    public bool IsFuelStation { get; set; }
    public double? FuelCapacityGallons { get; set; }
    public double? FuelWeightPerGallon { get; set; }
}
