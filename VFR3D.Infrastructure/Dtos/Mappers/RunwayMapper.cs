using VFR3D.Domain.Entities;

namespace VFR3D.Infrastructure.Dtos.Mappers;

public static class RunwayMapper
{
    public static RunwayDto ToDto(Runway runway)
    {
        return new RunwayDto
        {
            Id = runway.Id,
            RunwayId = runway.RunwayId,
            Length = runway.Length,
            Width = runway.Width,
            SurfaceTypeCode = runway.SurfaceTypeCode,
            SurfaceTreatmentCode = runway.SurfaceTreatmentCode,
            PavementClassification = runway.PavementClassification,
            EdgeLightIntensity = runway.EdgeLightIntensity,
            WeightBearingSingleWheel = runway.WeightBearingSingleWheel,
            WeightBearingDualWheel = runway.WeightBearingDualWheel,
            WeightBearingDualTandem = runway.WeightBearingDualTandem,
            WeightBearingDoubleDualTandem = runway.WeightBearingDoubleDualTandem,
            RunwayEnds = runway.RunwayEnds?.Select(ToDto).ToList() ?? new List<RunwayEndDto>()
        };
    }

    public static RunwayEndDto ToDto(RunwayEnd runwayEnd)
    {
        return new RunwayEndDto
        {
            Id = runwayEnd.Id,
            RunwayEndId = runwayEnd.RunwayEndId,
            TrueAlignment = runwayEnd.TrueAlignment,
            ApproachType = runwayEnd.ApproachType,
            RightHandTrafficPattern = runwayEnd.RightHandTrafficPattern,
            RunwayMarkingsType = runwayEnd.RunwayMarkingsType,
            RunwayMarkingsCondition = runwayEnd.RunwayMarkingsCondition,
            Latitude = runwayEnd.LatDecimal,
            Longitude = runwayEnd.LongDecimal,
            Elevation = runwayEnd.Elevation,
            ThresholdCrossingHeight = runwayEnd.ThresholdCrossingHeight,
            VisualGlidePathAngle = runwayEnd.VisualGlidePathAngle,
            DisplacedThresholdLatitude = runwayEnd.DisplacedThresholdLatDecimal,
            DisplacedThresholdLongitude = runwayEnd.DisplacedThresholdLongDecimal,
            DisplacedThresholdElevation = runwayEnd.DisplacedThresholdElev,
            DisplacedThresholdLength = runwayEnd.DisplacedThresholdLength,
            TouchdownZoneElevation = runwayEnd.TouchdownZoneElev,
            VisualGlideSlopeIndicator = runwayEnd.VisualGlideSlopeIndicator,
            RunwayVisualRangeEquipment = runwayEnd.RunwayVisualRangeEquipment,
            RunwayVisibilityValueEquipment = runwayEnd.RunwayVisibilityValueEquipment,
            ApproachLightSystem = runwayEnd.ApproachLightSystem,
            HasRunwayEndLights = runwayEnd.RunwayEndLights,
            HasCenterlineLights = runwayEnd.CenterlineLights,
            HasTouchdownZoneLights = runwayEnd.TouchdownZoneLights,
            ControllingObjectDescription = runwayEnd.ControllingObjectDescription,
            ControllingObjectMarkedLighted = runwayEnd.ControllingObjectMarkedLighted,
            ControllingObjectClearanceSlope = runwayEnd.ControllingObjectClearanceSlope,
            ControllingObjectHeightAboveRunway = runwayEnd.ControllingObjectHeightAboveRunway,
            ControllingObjectDistanceFromRunway = runwayEnd.ControllingObjectDistanceFromRunway,
            ControllingObjectCenterlineOffset = runwayEnd.ControllingObjectCenterlineOffset
        };
    }
}
