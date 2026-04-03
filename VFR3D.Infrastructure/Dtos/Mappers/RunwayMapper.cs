using VFR3D.Domain.Entities;
using VFR3D.Domain.Enums;

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
            SurfaceType = ParseSurfaceType(runway.SurfaceTypeCode),
            SurfaceTreatment = ParseSurfaceTreatment(runway.SurfaceTreatmentCode),
            PavementClassification = runway.PavementClassification,
            EdgeLightIntensity = ParseEdgeLightIntensity(runway.EdgeLightIntensity),
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
            ApproachType = ParseApproachType(runwayEnd.ApproachType),
            RightHandTrafficPattern = runwayEnd.RightHandTrafficPattern,
            MarkingsType = ParseMarkingsType(runwayEnd.RunwayMarkingsType),
            MarkingsCondition = ParseMarkingsCondition(runwayEnd.RunwayMarkingsCondition),
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
            VisualGlideSlopeIndicator = ParseVisualGlideSlopeIndicator(runwayEnd.VisualGlideSlopeIndicator),
            RunwayVisualRangeEquipment = runwayEnd.RunwayVisualRangeEquipment,
            RunwayVisibilityValueEquipment = runwayEnd.RunwayVisibilityValueEquipment,
            ApproachLightSystem = ParseApproachLightSystem(runwayEnd.ApproachLightSystem),
            HasRunwayEndLights = runwayEnd.RunwayEndLights,
            HasCenterlineLights = runwayEnd.CenterlineLights,
            HasTouchdownZoneLights = runwayEnd.TouchdownZoneLights,
            ControllingObjectDescription = runwayEnd.ControllingObjectDescription,
            ControllingObjectMarking = ParseControllingObjectMarking(runwayEnd.ControllingObjectMarkedLighted),
            ControllingObjectClearanceSlope = runwayEnd.ControllingObjectClearanceSlope,
            ControllingObjectHeightAboveRunway = runwayEnd.ControllingObjectHeightAboveRunway,
            ControllingObjectDistanceFromRunway = runwayEnd.ControllingObjectDistanceFromRunway,
            ControllingObjectCenterlineOffset = runwayEnd.ControllingObjectCenterlineOffset
        };
    }

    private static RunwaySurfaceType ParseSurfaceType(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return RunwaySurfaceType.Unknown;

        // Handle composite surface types (e.g., "ASPH-CONC") by taking the first type
        var primaryCode = code.Split('-', '/')[0].Trim().ToUpperInvariant();

        return primaryCode switch
        {
            "CONC" => RunwaySurfaceType.Concrete,
            "ASPH" => RunwaySurfaceType.Asphalt,
            "SNOW" => RunwaySurfaceType.Snow,
            "ICE" => RunwaySurfaceType.Ice,
            "MATS" => RunwaySurfaceType.Mats,
            "TREATED" or "TRTD" => RunwaySurfaceType.Treated,
            "GRAVEL" => RunwaySurfaceType.Gravel,
            "TURF" => RunwaySurfaceType.Turf,
            "DIRT" => RunwaySurfaceType.Dirt,
            "PEM" => RunwaySurfaceType.PartiallyPaved,
            "ROOF-TOP" or "ROOFTOP" => RunwaySurfaceType.Rooftop,
            "WATER" => RunwaySurfaceType.Water,
            "ALUMINUM" => RunwaySurfaceType.Aluminum,
            "BRICK" => RunwaySurfaceType.Brick,
            "CALICHE" => RunwaySurfaceType.Caliche,
            "CORAL" => RunwaySurfaceType.Coral,
            "DECK" => RunwaySurfaceType.Deck,
            "GRASS" => RunwaySurfaceType.Grass,
            "METAL" => RunwaySurfaceType.Metal,
            "NSTD" => RunwaySurfaceType.NonStandard,
            "OIL&CHIP" => RunwaySurfaceType.OilChip,
            "PSP" => RunwaySurfaceType.Psp,
            "SAND" => RunwaySurfaceType.Sand,
            "SOD" => RunwaySurfaceType.Sod,
            "STEEL" => RunwaySurfaceType.Steel,
            "WOOD" => RunwaySurfaceType.Wood,
            _ => RunwaySurfaceType.Unknown
        };
    }

    private static RunwaySurfaceTreatment ParseSurfaceTreatment(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return RunwaySurfaceTreatment.Unknown;

        return code.Trim().ToUpperInvariant() switch
        {
            "NONE" => RunwaySurfaceTreatment.None,
            "GRVD" => RunwaySurfaceTreatment.Grooved,
            "PFC" => RunwaySurfaceTreatment.PorousFrictionCourse,
            "AFSC" => RunwaySurfaceTreatment.AggregateFrictionSealCoat,
            "RFSC" => RunwaySurfaceTreatment.RubberizedFrictionSealCoat,
            "WC" => RunwaySurfaceTreatment.WireComb,
            _ => RunwaySurfaceTreatment.Unknown
        };
    }

    private static RunwayEdgeLightIntensity ParseEdgeLightIntensity(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return RunwayEdgeLightIntensity.Unknown;

        return code.Trim().ToUpperInvariant() switch
        {
            "NONE" => RunwayEdgeLightIntensity.None,
            "HIGH" => RunwayEdgeLightIntensity.High,
            "MED" => RunwayEdgeLightIntensity.Medium,
            "LOW" => RunwayEdgeLightIntensity.Low,
            "FLD" => RunwayEdgeLightIntensity.Flood,
            "NSTD" => RunwayEdgeLightIntensity.NonStandard,
            "PERI" => RunwayEdgeLightIntensity.Perimeter,
            "STRB" => RunwayEdgeLightIntensity.Strobe,
            _ => RunwayEdgeLightIntensity.Unknown
        };
    }

    private static InstrumentApproachType ParseApproachType(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return InstrumentApproachType.None;

        return code.Trim().ToUpperInvariant() switch
        {
            "ILS" => InstrumentApproachType.Ils,
            "MLS" => InstrumentApproachType.Mls,
            "SDF" => InstrumentApproachType.Sdf,
            "LOCALIZER" => InstrumentApproachType.Localizer,
            "LDA" => InstrumentApproachType.Lda,
            "ISMLS" => InstrumentApproachType.Ismls,
            "ILS/DME" => InstrumentApproachType.IlsDme,
            "SDF/DME" => InstrumentApproachType.SdfDme,
            "LOC/DME" => InstrumentApproachType.LocDme,
            "LOC/GS" => InstrumentApproachType.LocGs,
            "LDA/DME" => InstrumentApproachType.LdaDme,
            _ => InstrumentApproachType.Unknown
        };
    }

    private static RunwayMarkingsType ParseMarkingsType(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return RunwayMarkingsType.Unknown;

        return code.Trim().ToUpperInvariant() switch
        {
            "NONE" => RunwayMarkingsType.None,
            "PIR" => RunwayMarkingsType.PrecisionInstrument,
            "NPI" => RunwayMarkingsType.NonPrecisionInstrument,
            "BSC" => RunwayMarkingsType.Basic,
            "NRS" => RunwayMarkingsType.NumbersOnly,
            "NSTD" => RunwayMarkingsType.NonStandard,
            "BUOY" => RunwayMarkingsType.Buoys,
            "STOL" => RunwayMarkingsType.Stol,
            _ => RunwayMarkingsType.Unknown
        };
    }

    private static RunwayMarkingsCondition ParseMarkingsCondition(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return RunwayMarkingsCondition.Unknown;

        return code.Trim().ToUpperInvariant() switch
        {
            "G" => RunwayMarkingsCondition.Good,
            "F" => RunwayMarkingsCondition.Fair,
            "P" => RunwayMarkingsCondition.Poor,
            _ => RunwayMarkingsCondition.Unknown
        };
    }

    private static VisualGlideSlopeIndicatorType ParseVisualGlideSlopeIndicator(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return VisualGlideSlopeIndicatorType.Unknown;

        return code.Trim().ToUpperInvariant() switch
        {
            "NONE" or "N" => VisualGlideSlopeIndicatorType.None,
            // SAVASI
            "S2L" => VisualGlideSlopeIndicatorType.Savasi2BoxLeft,
            "S2R" => VisualGlideSlopeIndicatorType.Savasi2BoxRight,
            // VASI
            "V2L" => VisualGlideSlopeIndicatorType.Vasi2BoxLeft,
            "V2R" => VisualGlideSlopeIndicatorType.Vasi2BoxRight,
            "V4L" => VisualGlideSlopeIndicatorType.Vasi4BoxLeft,
            "V4R" => VisualGlideSlopeIndicatorType.Vasi4BoxRight,
            "V6L" => VisualGlideSlopeIndicatorType.Vasi6BoxLeft,
            "V6R" => VisualGlideSlopeIndicatorType.Vasi6BoxRight,
            "V12" => VisualGlideSlopeIndicatorType.Vasi12Box,
            "V16" => VisualGlideSlopeIndicatorType.Vasi16Box,
            // PAPI
            "P2L" => VisualGlideSlopeIndicatorType.Papi2LightLeft,
            "P2R" => VisualGlideSlopeIndicatorType.Papi2LightRight,
            "P4L" => VisualGlideSlopeIndicatorType.Papi4LightLeft,
            "P4R" => VisualGlideSlopeIndicatorType.Papi4LightRight,
            // Tri-Color
            "TRIL" => VisualGlideSlopeIndicatorType.TriColorLeft,
            "TRIR" => VisualGlideSlopeIndicatorType.TriColorRight,
            // Pulsating
            "PSIL" => VisualGlideSlopeIndicatorType.PulsatingLeft,
            "PSIR" => VisualGlideSlopeIndicatorType.PulsatingRight,
            // Panel
            "PNIL" => VisualGlideSlopeIndicatorType.PanelLeft,
            "PNIR" => VisualGlideSlopeIndicatorType.PanelRight,
            // Other
            "NSTD" => VisualGlideSlopeIndicatorType.NonStandard,
            "PVT" => VisualGlideSlopeIndicatorType.PrivateUse,
            "VAS" => VisualGlideSlopeIndicatorType.NonSpecificVasi,
            _ => VisualGlideSlopeIndicatorType.Unknown
        };
    }

    private static ApproachLightSystemType ParseApproachLightSystem(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return ApproachLightSystemType.None;

        return code.Trim().ToUpperInvariant() switch
        {
            "NONE" => ApproachLightSystemType.None,
            "AFOVRN" => ApproachLightSystemType.AirForceOverrun,
            "ALSAF" => ApproachLightSystemType.Alsaf,
            "ALSF1" => ApproachLightSystemType.Alsf1,
            "ALSF2" => ApproachLightSystemType.Alsf2,
            "MALS" => ApproachLightSystemType.Mals,
            "MALSF" => ApproachLightSystemType.Malsf,
            "MALSR" => ApproachLightSystemType.Malsr,
            "RAIL" => ApproachLightSystemType.Rail,
            "SALS" => ApproachLightSystemType.Sals,
            "SALSF" => ApproachLightSystemType.Salsf,
            "SSALS" => ApproachLightSystemType.Ssals,
            "SSALF" => ApproachLightSystemType.Ssalf,
            "SSALR" => ApproachLightSystemType.Ssalr,
            "ODALS" => ApproachLightSystemType.Odals,
            "RLLS" => ApproachLightSystemType.Rlls,
            "MIL OVRN" => ApproachLightSystemType.MilitaryOverrun,
            "NSTD" => ApproachLightSystemType.NonStandard,
            _ => ApproachLightSystemType.Unknown
        };
    }

    private static ControllingObjectMarking ParseControllingObjectMarking(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return ControllingObjectMarking.Unknown;

        return code.Trim().ToUpperInvariant() switch
        {
            "NONE" => ControllingObjectMarking.None,
            "M" => ControllingObjectMarking.Marked,
            "L" => ControllingObjectMarking.Lighted,
            "ML" => ControllingObjectMarking.MarkedAndLighted,
            _ => ControllingObjectMarking.Unknown
        };
    }
}
