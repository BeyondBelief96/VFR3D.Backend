using VFR3D.Domain.Enums;

namespace VFR3D.Infrastructure.Dtos;

public record RunwayDto
{
    public Guid Id { get; init; }
    public string RunwayId { get; init; } = string.Empty;
    public int? Length { get; init; }
    public int? Width { get; init; }
    public RunwaySurfaceType SurfaceType { get; init; }
    public RunwaySurfaceTreatment SurfaceTreatment { get; init; }
    public string? PavementClassification { get; init; }
    public RunwayEdgeLightIntensity EdgeLightIntensity { get; init; }
    public int? WeightBearingSingleWheel { get; init; }
    public int? WeightBearingDualWheel { get; init; }
    public int? WeightBearingDualTandem { get; init; }
    public int? WeightBearingDoubleDualTandem { get; init; }
    public List<RunwayEndDto> RunwayEnds { get; init; } = new();
}

public record RunwayEndDto
{
    public Guid Id { get; init; }
    public string RunwayEndId { get; init; } = string.Empty;
    public int? TrueAlignment { get; init; }
    public InstrumentApproachType ApproachType { get; init; }
    public bool RightHandTrafficPattern { get; init; }
    public RunwayMarkingsType MarkingsType { get; init; }
    public RunwayMarkingsCondition MarkingsCondition { get; init; }
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
    public decimal? Elevation { get; init; }
    public decimal? ThresholdCrossingHeight { get; init; }
    public decimal? VisualGlidePathAngle { get; init; }
    public decimal? DisplacedThresholdLatitude { get; init; }
    public decimal? DisplacedThresholdLongitude { get; init; }
    public decimal? DisplacedThresholdElevation { get; init; }
    public int? DisplacedThresholdLength { get; init; }
    public decimal? TouchdownZoneElevation { get; init; }
    public VisualGlideSlopeIndicatorType VisualGlideSlopeIndicator { get; init; }
    public string? RunwayVisualRangeEquipment { get; init; }
    public bool RunwayVisibilityValueEquipment { get; init; }
    public ApproachLightSystemType ApproachLightSystem { get; init; }
    public bool HasRunwayEndLights { get; init; }
    public bool HasCenterlineLights { get; init; }
    public bool HasTouchdownZoneLights { get; init; }
    public string? ControllingObjectDescription { get; init; }
    public ControllingObjectMarking ControllingObjectMarking { get; init; }
    public int? ControllingObjectClearanceSlope { get; init; }
    public int? ControllingObjectHeightAboveRunway { get; init; }
    public int? ControllingObjectDistanceFromRunway { get; init; }
    public string? ControllingObjectCenterlineOffset { get; init; }
}
