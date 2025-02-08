using CsvHelper.Configuration;
using VFR3D.Domain.Entities;
using VFR3D.Infrastructure.Services.NasrServices.Utils;

namespace VFR3D.Infrastructure.Services.NasrServices.Mappings
{
    public sealed class AirportBaseMap : ClassMap<Airport>
    {
        public AirportBaseMap()
        {
            Map(m => m.SiteNo).Name("SITE_NO");
            Map(m => m.EffDate).Name("EFF_DATE").TypeConverter<OptionalDateConverter>();
            Map(m => m.SiteTypeCode).Name("SITE_TYPE_CODE").Optional();
            Map(m => m.StateCode).Name("STATE_CODE").Optional();
            Map(m => m.ArptId).Name("ARPT_ID").Optional();
            Map(m => m.City).Name("CITY").Optional();
            Map(m => m.CountryCode).Name("COUNTRY_CODE").Optional();
            Map(m => m.RegionCode).Name("REGION_CODE").Optional();
            Map(m => m.AdoCode).Name("ADO_CODE").Optional();
            Map(m => m.StateName).Name("STATE_NAME").Optional();
            Map(m => m.CountyName).Name("COUNTY_NAME").Optional();
            Map(m => m.CountyAssocState).Name("COUNTY_ASSOC_STATE").Optional();
            Map(m => m.ArptName).Name("ARPT_NAME").Optional();
            Map(m => m.OwnershipTypeCode).Name("OWNERSHIP_TYPE_CODE").Optional();
            Map(m => m.FacilityUseCode).Name("FACILITY_USE_CODE").Optional();
            Map(m => m.LatDecimal).Name("LAT_DECIMAL").TypeConverter<OptionalDecimalConverter>().Optional();
            Map(m => m.LongDecimal).Name("LONG_DECIMAL").TypeConverter<OptionalDecimalConverter>().Optional();
            Map(m => m.LatDeg).Name("LAT_DEG").TypeConverter<OptionalIntConverter>().Optional();
            Map(m => m.LatMin).Name("LAT_MIN").TypeConverter<OptionalIntConverter>().Optional();
            Map(m => m.LatSec).Name("LAT_SEC").TypeConverter<OptionalDecimalConverter>().Optional();
            Map(m => m.LatHemis).Name("LAT_HEMIS").Optional();
            Map(m => m.LongDeg).Name("LONG_DEG").TypeConverter<OptionalIntConverter>().Optional();
            Map(m => m.LongMin).Name("LONG_MIN").TypeConverter<OptionalIntConverter>().Optional();
            Map(m => m.LongSec).Name("LONG_SEC").TypeConverter<OptionalDecimalConverter>().Optional();
            Map(m => m.LongHemis).Name("LONG_HEMIS").Optional();
            Map(m => m.SurveyMethodCode).Name("SURVEY_METHOD_CODE").Optional();
            Map(m => m.Elev).Name("ELEV").TypeConverter<OptionalDecimalConverter>().Optional();
            Map(m => m.ElevMethodCode).Name("ELEV_METHOD_CODE").Optional();
            Map(m => m.MagVarn).Name("MAG_VARN").TypeConverter<OptionalDecimalConverter>().Optional();
            Map(m => m.MagHemis).Name("MAG_HEMIS").Optional();
            Map(m => m.MagVarnYear).Name("MAG_VARN_YEAR").TypeConverter<OptionalIntConverter>().Optional();
            Map(m => m.Tpa).Name("TPA").TypeConverter<OptionalIntConverter>().Optional();
            Map(m => m.ChartName).Name("CHART_NAME").Optional();
            Map(m => m.DistCityToAirport).Name("DIST_CITY_TO_AIRPORT").TypeConverter<OptionalDecimalConverter>().Optional();
            Map(m => m.DirectionCode).Name("DIRECTION_CODE").Optional();
            Map(m => m.Acreage).Name("ACREAGE").TypeConverter<OptionalIntConverter>().Optional();
            Map(m => m.RespArtccId).Name("RESP_ARTCC_ID").Optional();
            Map(m => m.FssOnArptFlag).Name("FSS_ON_ARPT_FLAG").Optional();
            Map(m => m.FssId).Name("FSS_ID").Optional();
            Map(m => m.FssName).Name("FSS_NAME").Optional();
            Map(m => m.NotamId).Name("NOTAM_ID").Optional();
            Map(m => m.NotamFlag).Name("NOTAM_FLAG").Optional();
            Map(m => m.ActivationDate).Name("ACTIVATION_DATE").Optional();
            Map(m => m.ArptStatus).Name("ARPT_STATUS").Optional();
            Map(m => m.NaspCode).Name("NASP_CODE").Optional();
            Map(m => m.CustomsFlag).Name("CUST_FLAG").Optional();
            Map(m => m.LndgRightsFlag).Name("LNDG_RIGHTS_FLAG").Optional();
            Map(m => m.JointUseFlag).Name("JOINT_USE_FLAG").Optional();
            Map(m => m.MilLndgFlag).Name("MIL_LNDG_FLAG").Optional();
            Map(m => m.InspectMethodCode).Name("INSPECT_METHOD_CODE").Optional();
            Map(m => m.InspectorCode).Name("INSPECTOR_CODE").Optional();
            Map(m => m.LastInspection).Name("LAST_INSPECTION").TypeConverter<OptionalDateConverter>().Optional();
            Map(m => m.LastInfoResponse).Name("LAST_INFO_RESPONSE").TypeConverter<OptionalDateConverter>().Optional();
            Map(m => m.FuelTypes).Name("FUEL_TYPES").Optional();
            Map(m => m.AirframeRepairSerCode).Name("AIRFRAME_REPAIR_SER_CODE").Optional();
            Map(m => m.PwrPlantRepairSer).Name("PWR_PLANT_REPAIR_SER").Optional();
            Map(m => m.BottledOxyType).Name("BOTTLED_OXY_TYPE").Optional();
            Map(m => m.BulkOxyType).Name("BULK_OXY_TYPE").Optional();
            Map(m => m.LgtSked).Name("LGT_SKED").Optional();
            Map(m => m.BcnLgtSked).Name("BCN_LGT_SKED").Optional();
            Map(m => m.TwrTypeCode).Name("TWR_TYPE_CODE").Optional();
            Map(m => m.SegCircleMkrFlag).Name("SEG_CIRCLE_MKR_FLAG").Optional();
            Map(m => m.BcnLensColor).Name("BCN_LENS_COLOR").Optional();
            Map(m => m.LndgFeeFlag).Name("LNDG_FEE_FLAG").Optional();
            Map(m => m.MedicalUseFlag).Name("MEDICAL_USE_FLAG").Optional();
            Map(m => m.ArptPsnSource).Name("ARPT_PSN_SOURCE").Optional();
            Map(m => m.PositionSrcDate).Name("POSITION_SRC_DATE").TypeConverter<OptionalDateConverter>().Optional();
            Map(m => m.ArptElevSource).Name("ARPT_ELEV_SOURCE").Optional();
            Map(m => m.ElevationSrcDate).Name("ELEVATION_SRC_DATE").TypeConverter<OptionalDateConverter>().Optional();
            Map(m => m.ContrFuelAvbl).Name("CONTR_FUEL_AVBL").Optional();
            Map(m => m.TrnsStrgBuoyFlag).Name("TRNS_STRG_BUOY_FLAG").Optional();
            Map(m => m.TrnsStrgHgrFlag).Name("TRNS_STRG_HGR_FLAG").Optional();
            Map(m => m.TrnsStrgTieFlag).Name("TRNS_STRG_TIE_FLAG").Optional();
            Map(m => m.OtherServices).Name("OTHER_SERVICES").Optional();
            Map(m => m.WindIndcrFlag).Name("WIND_INDCR_FLAG").Optional();
            Map(m => m.IcaoId).Name("ICAO_ID").Optional();
            Map(m => m.MinOpNetwork).Name("MIN_OP_NETWORK").Optional();
            Map(m => m.UserFeeFlag).Name("USER_FEE_FLAG").Optional();
            Map(m => m.Cta).Name("CTA").Optional();
        }
    }

    public sealed class AirportAttendanceMap : ClassMap<Airport>
    {
        public AirportAttendanceMap()
        {
            Map(m => m.SiteNo).Name("SITE_NO");
            Map(m => m.EffDate).Name("EFF_DATE").TypeConverter<OptionalDateConverter>();
            Map(m => m.SiteTypeCode).Name("SITE_TYPE_CODE").Optional();
            Map(m => m.StateCode).Name("STATE_CODE").Optional();
            Map(m => m.ArptId).Name("ARPT_ID").Optional();
            Map(m => m.City).Name("CITY").Optional();
            Map(m => m.CountryCode).Name("COUNTRY_CODE").Optional();
            Map(m => m.AttendanceMonth).Name("MONTH").Optional();
            Map(m => m.AttendanceDay).Name("DAY").Optional();
            Map(m => m.AttendanceHours).Name("HOUR").Optional();
        }
    }

    public sealed class AirportContactMap : ClassMap<Airport>
    {
        public AirportContactMap()
        {
            Map(m => m.SiteNo).Name("SITE_NO");
            Map(m => m.EffDate).Name("EFF_DATE").TypeConverter<OptionalDateConverter>();
            Map(m => m.SiteTypeCode).Name("SITE_TYPE_CODE").Optional();
            Map(m => m.StateCode).Name("STATE_CODE").Optional();
            Map(m => m.ArptId).Name("ARPT_ID").Optional();
            Map(m => m.City).Name("CITY").Optional();
            Map(m => m.CountryCode).Name("COUNTRY_CODE").Optional();
            Map(m => m.ContactTitle).Name("TITLE").Optional();
            Map(m => m.ContactName).Name("NAME").Optional();
            Map(m => m.ContactAddress1).Name("ADDRESS1").Optional();
            Map(m => m.ContactAddress2).Name("ADDRESS2").Optional();
            Map(m => m.ContactCity).Name("TITLE_CITY").Optional();
            Map(m => m.ContactState).Name("STATE").Optional();
            Map(m => m.ContactZipCode).Name("ZIP_CODE").Optional();
            Map(m => m.ContactZipPlusFour).Name("ZIP_PLUS_FOUR").Optional();
            Map(m => m.ContactPhoneNumber).Name("PHONE_NO").Optional();
        }
    }
}
