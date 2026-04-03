using CsvHelper.Configuration;
using VFR3D.Domain.Entities;
using VFR3D.Infrastructure.Services.CronJobServices.NasrServices.Utils;

namespace VFR3D.Infrastructure.Services.CronJobServices.NasrServices.Mappings;

public sealed class RunwayMap : ClassMap<Runway>
{
    public RunwayMap()
    {
        Map(m => m.SiteNo).Name("SITE_NO");
        Map(m => m.RunwayId).Name("RWY_ID");
        Map(m => m.Length).Name("RWY_LEN").TypeConverter<OptionalIntConverter>().Optional();
        Map(m => m.Width).Name("RWY_WIDTH").TypeConverter<OptionalIntConverter>().Optional();
        Map(m => m.SurfaceTypeCode).Name("SURFACE_TYPE_CODE").Optional();
        Map(m => m.SurfaceTreatmentCode).Name("SURFACE_TREATMENT_CODE").Optional();
        Map(m => m.PavementClassification).Name("PCN").Optional();
        Map(m => m.EdgeLightIntensity).Name("RWY_LGT_CODE").Optional();
        Map(m => m.WeightBearingSingleWheel).Name("GROSS_WT_SW").TypeConverter<OptionalIntConverter>().Optional();
        Map(m => m.WeightBearingDualWheel).Name("GROSS_WT_DW").TypeConverter<OptionalIntConverter>().Optional();
        Map(m => m.WeightBearingDualTandem).Name("GROSS_WT_DTW").TypeConverter<OptionalIntConverter>().Optional();
        Map(m => m.WeightBearingDoubleDualTandem).Name("GROSS_WT_DDT").TypeConverter<OptionalIntConverter>().Optional();
    }
}

public sealed class RunwayEndMap : ClassMap<RunwayEnd>
{
    public RunwayEndMap()
    {
        Map(m => m.SiteNo).Name("SITE_NO");
        Map(m => m.RunwayIdRef).Name("RWY_ID");
        Map(m => m.RunwayEndId).Name("RWY_END_ID");
        Map(m => m.TrueAlignment).Name("TRUE_ALIGNMENT").TypeConverter<OptionalIntConverter>().Optional();
        Map(m => m.ApproachType).Name("APCH_TYPE_CODE").Optional();
        Map(m => m.RightHandTrafficPattern).Name("RIGHT_HAND_TRAFFIC_PAT_FLAG").TypeConverter<YesNoToBoolConverter>().Optional();
        Map(m => m.RunwayMarkingsType).Name("RWY_MKG_TYPE_CODE").Optional();
        Map(m => m.RunwayMarkingsCondition).Name("RWY_MKG_COND").Optional();
        Map(m => m.LatDecimal).Name("LAT_DECIMAL").TypeConverter<OptionalDecimalConverter>().Optional();
        Map(m => m.LongDecimal).Name("LONG_DECIMAL").TypeConverter<OptionalDecimalConverter>().Optional();
        Map(m => m.Elevation).Name("RWY_END_ELEV").TypeConverter<OptionalDecimalConverter>().Optional();
        Map(m => m.ThresholdCrossingHeight).Name("THR_CROSSING_HGT").TypeConverter<OptionalDecimalConverter>().Optional();
        Map(m => m.VisualGlidePathAngle).Name("VISUAL_GLIDE_PATH_ANGLE").TypeConverter<OptionalDecimalConverter>().Optional();
        Map(m => m.DisplacedThresholdLatDecimal).Name("DSPLCD_THR_LAT_DECIMAL").TypeConverter<OptionalDecimalConverter>().Optional();
        Map(m => m.DisplacedThresholdLongDecimal).Name("DSPLCD_THR_LONG_DECIMAL").TypeConverter<OptionalDecimalConverter>().Optional();
        Map(m => m.DisplacedThresholdElev).Name("DSPLCD_THR_ELEV").TypeConverter<OptionalDecimalConverter>().Optional();
        Map(m => m.DisplacedThresholdLength).Name("DSPLCD_THR_LEN").TypeConverter<OptionalIntConverter>().Optional();
        Map(m => m.TouchdownZoneElev).Name("TDZ_ELEV").TypeConverter<OptionalDecimalConverter>().Optional();
        Map(m => m.VisualGlideSlopeIndicator).Name("VGSI_CODE").Optional();
        Map(m => m.RunwayVisualRangeEquipment).Name("RVR_EQUIP_CODE").Optional();
        Map(m => m.RunwayVisibilityValueEquipment).Name("RVV_EQUIP_FLAG").TypeConverter<YesNoToBoolConverter>().Optional();
        Map(m => m.ApproachLightSystem).Name("APCH_LGT_SYSTEM_CODE").Optional();
        Map(m => m.RunwayEndLights).Name("RWY_END_LGTS_FLAG").TypeConverter<YesNoToBoolConverter>().Optional();
        Map(m => m.CenterlineLights).Name("CNTRLN_LGTS_AVBL_FLAG").TypeConverter<YesNoToBoolConverter>().Optional();
        Map(m => m.TouchdownZoneLights).Name("TDZ_LGT_AVBL_FLAG").TypeConverter<YesNoToBoolConverter>().Optional();
        Map(m => m.ControllingObjectDescription).Name("CNTL_OBJ_DESC").Optional();
        Map(m => m.ControllingObjectMarkedLighted).Name("CNTL_OBJ_MKD_LGT").Optional();
        Map(m => m.ControllingObjectClearanceSlope).Name("CNTL_OBJ_SLOPE").TypeConverter<OptionalIntConverter>().Optional();
        Map(m => m.ControllingObjectHeightAboveRunway).Name("CNTL_OBJ_HGT").TypeConverter<OptionalIntConverter>().Optional();
        Map(m => m.ControllingObjectDistanceFromRunway).Name("CNTL_OBJ_DIST").TypeConverter<OptionalIntConverter>().Optional();
        Map(m => m.ControllingObjectCenterlineOffset).Name("CNTL_OBJ_OFFSET").Optional();
    }
}
