namespace VFR3D.Infrastructure.Dtos;

public record RunwayDto
{
    public Guid Id { get; init; }
    public string RunwayId { get; init; } = string.Empty;
    public int? Length { get; init; }
    public int? Width { get; init; }
    public string? SurfaceTypeCode { get; init; }
    public string? SurfaceTreatmentCode { get; init; }
    public string? PavementClassification { get; init; }
    public string? EdgeLightIntensity { get; init; }
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
    public string? ApproachType { get; init; }
    public bool RightHandTrafficPattern { get; init; }
    public string? RunwayMarkingsType { get; init; }
    public string? RunwayMarkingsCondition { get; init; }
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
    public string? VisualGlideSlopeIndicator { get; init; }
    public string? RunwayVisualRangeEquipment { get; init; }
    public bool RunwayVisibilityValueEquipment { get; init; }
    public string? ApproachLightSystem { get; init; }
    public bool HasRunwayEndLights { get; init; }
    public bool HasCenterlineLights { get; init; }
    public bool HasTouchdownZoneLights { get; init; }
    public string? ControllingObjectDescription { get; init; }
    public string? ControllingObjectMarkedLighted { get; init; }
    public int? ControllingObjectClearanceSlope { get; init; }
    public int? ControllingObjectHeightAboveRunway { get; init; }
    public int? ControllingObjectDistanceFromRunway { get; init; }
    public string? ControllingObjectCenterlineOffset { get; init; }
}
