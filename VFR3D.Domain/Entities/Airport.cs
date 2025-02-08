using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VFR3D.Domain.Entities;

[Table("airports")]
public class Airport
{

    [Key]
    [Column("site_no", TypeName = "varchar(9)")]
    public string SiteNo { get; set; } = string.Empty;

    [Column("eff_date")]
    public DateTime EffDate { get; set; }

    [Column("site_type_code", TypeName = "varchar(1)")]
    public string? SiteTypeCode { get; set; } = string.Empty;

    [Column("state_code", TypeName = "varchar(2)")]
    public string? StateCode { get; set; }

    [Column("arpt_id", TypeName = "varchar(4)")]
    public string? ArptId { get; set; } = string.Empty;

    [Column("city", TypeName = "varchar(40)")]
    public string? City { get; set; } = string.Empty;

    [Column("country_code", TypeName = "varchar(2)")]
    public string? CountryCode { get; set; } = string.Empty;

    [Column("region_code", TypeName = "varchar(3)")]
    public string? RegionCode { get; set; }

    [Column("ado_code", TypeName = "varchar(3)")]
    public string? AdoCode { get; set; }

    [Column("state_name", TypeName = "varchar(30)")]
    public string? StateName { get; set; }

    [Column("county_name", TypeName = "varchar(21)")]
    public string? CountyName { get; set; } = string.Empty;

    [Column("county_assoc_state", TypeName = "varchar(2)")]
    public string? CountyAssocState { get; set; } = string.Empty;

    [Column("arpt_name", TypeName = "varchar(50)")]
    public string? ArptName { get; set; } = string.Empty;

    [Column("ownership_type_code", TypeName = "varchar(2)")]
    public string? OwnershipTypeCode { get; set; } = string.Empty;

    [Column("facility_use_code", TypeName = "varchar(2)")]
    public string? FacilityUseCode { get; set; } = string.Empty;

    [Column("lat_decimal", TypeName = "decimal(10,8)")]
    public decimal? LatDecimal { get; set; }

    [Column("long_decimal", TypeName = "decimal(11,8)")]
    public decimal? LongDecimal { get; set; }

    [Column("lat_deg", TypeName = "int")]
    public int? LatDeg { get; set; }

    [Column("lat_min", TypeName = "int")]
    public int? LatMin { get; set; }

    [Column("lat_sec", TypeName = "decimal(6,2)")]
    public decimal? LatSec { get; set; }

    [Column("lat_hemis", TypeName = "varchar(1)")]
    public string? LatHemis { get; set; }

    [Column("long_deg", TypeName = "int")]
    public int? LongDeg { get; set; }

    [Column("long_min", TypeName = "int")]
    public int? LongMin { get; set; }

    [Column("long_sec", TypeName = "decimal(6,2)")]
    public decimal? LongSec { get; set; }

    [Column("long_hemis", TypeName = "varchar(1)")]
    public string? LongHemis { get; set; }

    [Column("survey_method_code", TypeName = "varchar(1)")]
    public string? SurveyMethodCode { get; set; }

    [Column("elev", TypeName = "decimal(6,1)")]
    public decimal? Elev { get; set; }

    [Column("elev_method_code", TypeName = "varchar(1)")]
    public string? ElevMethodCode { get; set; }

    [Column("mag_varn", TypeName = "decimal(2,0)")]
    public decimal? MagVarn { get; set; }

    [Column("mag_hemis", TypeName = "varchar(1)")]
    public string? MagHemis { get; set; }

    [Column("mag_varn_year")]
    public int? MagVarnYear { get; set; }

    [Column("tpa")]
    public int? Tpa { get; set; }

    [Column("chart_name", TypeName = "varchar(30)")]
    public string? ChartName { get; set; }

    [Column("dist_city_to_airport", TypeName = "decimal(2,0)")]
    public decimal? DistCityToAirport { get; set; }

    [Column("direction_code", TypeName = "varchar(3)")]
    public string? DirectionCode { get; set; }

    [Column("acreage")]
    public int? Acreage { get; set; }

    [Column("resp_artcc_id", TypeName = "varchar(4)")]
    public string? RespArtccId { get; set; } = string.Empty;

    [Column("fss_on_arpt_flag", TypeName = "varchar(1)")]
    public string? FssOnArptFlag { get; set; }

    [Column("fss_id", TypeName = "varchar(4)")]
    public string? FssId { get; set; } = string.Empty;

    [Column("fss_name", TypeName = "varchar(30)")]
    public string? FssName { get; set; } = string.Empty;

    [Column("notam_id", TypeName = "varchar(4)")]
    public string? NotamId { get; set; }

    [Column("notam_flag", TypeName = "varchar(1)")]
    public string? NotamFlag { get; set; }

    [Column("activation_date", TypeName = "varchar(7)")]
    public string? ActivationDate { get; set; }

    [Column("arpt_status", TypeName = "varchar(2)")]
    public string? ArptStatus { get; set; } = string.Empty;

    [Column("nasp_code", TypeName = "varchar(7)")]
    public string? NaspCode { get; set; }

    [Column("customs_flag", TypeName = "varchar(1)")]
    public string? CustomsFlag { get; set; }

    [Column("lndg_rights_flag", TypeName = "varchar(1)")]
    public string? LndgRightsFlag { get; set; }

    [Column("joint_use_flag", TypeName = "varchar(1)")]
    public string? JointUseFlag { get; set; }

    [Column("mil_lndg_flag", TypeName = "varchar(1)")]
    public string? MilLndgFlag { get; set; }

    [Column("inspect_method_code", TypeName = "varchar(1)")]
    public string? InspectMethodCode { get; set; }

    [Column("inspector_code", TypeName = "varchar(1)")]
    public string? InspectorCode { get; set; } = string.Empty;

    [Column("last_inspection")]
    public DateTime? LastInspection { get; set; }

    [Column("last_info_response")]
    public DateTime? LastInfoResponse { get; set; }

    [Column("fuel_types", TypeName = "varchar(40)")]
    public string? FuelTypes { get; set; }

    [Column("airframe_repair_ser_code", TypeName = "varchar(5)")]
    public string? AirframeRepairSerCode { get; set; }

    [Column("pwr_plant_repair_ser", TypeName = "varchar(5)")]
    public string? PwrPlantRepairSer { get; set; }

    [Column("bottled_oxy_type", TypeName = "varchar(8)")]
    public string? BottledOxyType { get; set; }

    [Column("bulk_oxy_type", TypeName = "varchar(8)")]
    public string? BulkOxyType { get; set; }

    [Column("lgt_sked", TypeName = "varchar(7)")]
    public string? LgtSked { get; set; }

    [Column("bcn_lgt_sked", TypeName = "varchar(7)")]
    public string? BcnLgtSked { get; set; }

    [Column("twr_type_code", TypeName = "varchar(12)")]
    public string? TwrTypeCode { get; set; } = string.Empty;

    [Column("seg_circle_mkr_flag", TypeName = "varchar(3)")]
    public string? SegCircleMkrFlag { get; set; }

    [Column("bcn_lens_color", TypeName = "varchar(3)")]
    public string? BcnLensColor { get; set; }

    [Column("lndg_fee_flag", TypeName = "varchar(1)")]
    public string? LndgFeeFlag { get; set; }

    [Column("medical_use_flag", TypeName = "varchar(1)")]
    public string? MedicalUseFlag { get; set; }

    [Column("arpt_psn_source", TypeName = "varchar(16)")]
    public string? ArptPsnSource { get; set; }

    [Column("position_src_date")]
    public DateTime? PositionSrcDate { get; set; }

    [Column("arpt_elev_source", TypeName = "varchar(16)")]
    public string? ArptElevSource { get; set; }

    [Column("elevation_src_date")]
    public DateTime? ElevationSrcDate { get; set; }

    [Column("contr_fuel_avbl", TypeName = "varchar(1)")]
    public string? ContrFuelAvbl { get; set; }

    [Column("trns_strg_buoy_flag", TypeName = "varchar(1)")]
    public string? TrnsStrgBuoyFlag { get; set; }

    [Column("trns_strg_hgr_flag", TypeName = "varchar(1)")]
    public string? TrnsStrgHgrFlag { get; set; }

    [Column("trns_strg_tie_flag", TypeName = "varchar(1)")]
    public string? TrnsStrgTieFlag { get; set; }

    [Column("other_services", TypeName = "varchar(110)")]
    public string? OtherServices { get; set; }

    [Column("wind_indcr_flag", TypeName = "varchar(3)")]
    public string? WindIndcrFlag { get; set; }

    [Column("icao_id", TypeName = "varchar(7)")]
    public string? IcaoId { get; set; }

    [Column("min_op_network", TypeName = "varchar(1)")]
    public string? MinOpNetwork { get; set; } = string.Empty;

    [Column("user_fee_flag", TypeName = "varchar(26)")]
    public string? UserFeeFlag { get; set; }

    [Column("cta", TypeName = "varchar(4)")]
    public string? Cta { get; set; }

    [Column("sked_seq_no")]
    public int? SkedSeqNo { get; set; }

    [Column("attendance_month", TypeName = "varchar(50)")]
    public string? AttendanceMonth { get; set; }

    [Column("attendance_day", TypeName = "varchar(16)")]
    public string? AttendanceDay { get; set; }

    [Column("attendance_hours", TypeName = "varchar(40)")]
    public string? AttendanceHours { get; set; }

    [Column("contact_title", TypeName = "varchar(10)")]
    public string? ContactTitle { get; set; }

    [Column("contact_name", TypeName = "varchar(35)")]
    public string? ContactName { get; set; }

    [Column("contact_address1", TypeName = "varchar(35)")]
    public string? ContactAddress1 { get; set; }

    [Column("contact_address2", TypeName = "varchar(35)")]
    public string? ContactAddress2 { get; set; }

    [Column("contact_city", TypeName = "varchar(30)")]
    public string? ContactCity { get; set; }

    [Column("contact_state", TypeName = "varchar(2)")]
    public string? ContactState { get; set; }

    [Column("contact_zip_code", TypeName = "varchar(5)")]
    public string? ContactZipCode { get; set; }

    [Column("contact_zip_plus_four", TypeName = "varchar(4)")]
    public string? ContactZipPlusFour { get; set; }

    [Column("contact_phone_number", TypeName = "varchar(16)")]
    public string? ContactPhoneNumber { get; set; }
}