using VFR3D.Domain.Enums;

namespace VFR3D.Infrastructure.Dtos.WeightBalance;

public record LoadingStationDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public double MaxWeight { get; init; }

    /// <summary>
    /// First point on the loading graph line for this station.
    /// </summary>
    public LoadingGraphPointDto Point1 { get; init; } = new();

    /// <summary>
    /// Second point on the loading graph line for this station.
    /// </summary>
    public LoadingGraphPointDto Point2 { get; init; } = new();

    public LoadingStationType StationType { get; init; } = LoadingStationType.Standard;

    // Fuel station properties (when StationType == Fuel)
    public double? FuelCapacityGallons { get; init; }
    public double? FuelWeightPerGallon { get; init; }

    // Oil station properties (when StationType == Oil)
    public double? OilCapacityQuarts { get; init; }
    public double? OilWeightPerQuart { get; init; }
}
