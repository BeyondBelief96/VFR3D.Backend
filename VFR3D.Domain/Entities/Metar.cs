using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VFR3D.Domain.ValueObjects.Metar;

namespace Vfr3d.Domain.Entities;

[Table("metar")]
public class Metar
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Raw text of observation ex: KORD 032151Z 23006KT 10SM BKN110 OVC250 14/03 A3000 RMK AO2 SLP162 VIRGA OHD T01440028
    /// </summary>
    [Column("raw_text")]
    public string? RawText { get; set; }

    /// <summary>
    /// ICAO identifier ex: KORD
    /// </summary>
    [Column("station_id")]
    public string? StationId { get; set; }

    /// <summary>
    /// The observation time (ISO 8601 date format) ex: 2023-11-06T20:51:00Z
    /// </summary>
    [Column("observation_time")]
    public string? ObservationTime { get; set; }

    /// <summary>
    /// Latitude of site in degrees ex: 41.9602
    /// </summary>
    [Column("latitude")]
    public float? Latitude { get; set; }

    /// <summary>
    /// Longitude of site in degrees ex: -87.9316
    /// </summary>
    [Column("longitude")]
    public float? Longitude { get; set; }

    /// <summary>
    /// Temperature in degrees Celsius ex: 14.4
    /// </summary>
    [Column("temp_c")]
    public float? TempC { get; set; }

    /// <summary>
    /// Dewpoint temperature in degrees Celsius ex: 2.8
    /// </summary>
    [Column("dewpoint_c")]
    public float? DewpointC { get; set; }

    /// <summary>
    /// Wind direction in degrees or VRB for variable winds ex: 230, VRB
    /// </summary>
    [Column("wind_dir_degrees")]
    public string? WindDirDegrees { get; set; }

    /// <summary>
    /// Wind speed in knots.
    /// </summary>
    [Column("wind_speed_kt")]
    public int? WindSpeedKt { get; set; }

    /// <summary>
    /// Wind gust speed in knots, if available.
    /// </summary>
    [Column("wind_gust_kt")]
    public int? WindGustKt { get; set; }

    /// <summary>
    /// Visibility in statute miles, 10+ is greater than 10 sm ex: 3, 10+
    /// </summary>
    [Column("visibility_statute_mi")]
    public string? VisibilityStatuteMi { get; set; }

    /// <summary>
    /// Altimeter setting in inches of mercury ex: 29.92
    /// </summary>
    [Column("altim_in_hg")]
    public float? AltimInHg { get; set; }

    /// <summary>
    /// Altimer setting in millibars ex: 1013.25
    /// </summary>
    [Column("sea_level_pressure_mb")]
    public float? SeaLevelPressureMb { get; set; }

    /// <summary>
    /// Quality control flags associated with the METAR data.
    /// </summary>
    [Column("quality_control_flags", TypeName = "jsonb")]
    public MetarQualityControlFlags? QualityControlFlags { get; set; }

    [Column("wx_string")]
    public string? WxString { get; set; }

    /// <summary>
    /// Maximum of 4 sky conditions as per XSD schema
    /// </summary>
    [Column("sky_condition", TypeName = "jsonb")]
    public List<MetarSkyCondition>? SkyCondition { get; set; }

    /// <summary>
    /// Flight category, if available. ex: VFR, MVFR, IFR, LIFR
    /// </summary>
    [Column("flight_category")]
    public string? FlightCategory { get; set; }

    [Column("three_hr_pressure_tendency_mb")]
    public float? ThreeHrPressureTendencyMb { get; set; }

    /// <summary>
    /// Maximum temperature in degrees Celsius, if available. ex: 15.0
    /// </summary>
    [Column("maxT_c")]
    public float? MaxTC { get; set; }

    /// <summary>
    /// Minimum temperature in degrees Celsius, if available. ex: 3.0
    /// </summary>
    [Column("minT_c")]
    public float? MinTC { get; set; }

    /// <summary>
    /// Maximum temperature in degrees Celsius predicted over the next 24 hours, if available. ex: 15.0
    /// </summary>
    [Column("maxT24hr_c")]
    public float? MaxT24hrC { get; set; }

    /// <summary>
    /// Minimum temperature in degrees Celsius predicted over the next 24 hours, if available. ex: 3.0
    [Column("minT24hr_c")]
    public float? MinT24hrC { get; set; }

    /// <summary>
    /// Expected precipitation in inches over the last hour, if available. ex: 0.0
    /// </summary>
    [Column("precip_in")]
    public float? PrecipIn { get; set; }

    /// <summary>
    /// Expected precipitation in inches over the next 3 hour, if available. ex: 0.0
    /// </summary>
    [Column("pcp3hr_in")]
    public float? Pcp3hrIn { get; set; }

    /// <summary>
    /// Expected precipitation in inches over the next 6 hours, if available. ex: 0.0
    /// </summary>
    [Column("pcp6hr_in")]
    public float? Pcp6hrIn { get; set; }

    /// <summary>
    /// Expected precipitation in inches over the next 24 hours, if available. ex: 0.0
    /// </summary>
    [Column("pcp24hr_in")]
    public float? Pcp24hrIn { get; set; }

    /// <summary>
    /// Snow depth in inches, if available. ex: 0.0
    /// </summary>
    [Column("snow_in")]
    public float? SnowIn { get; set; }

    /// <summary>
    /// Vertical visibility in feet, if available. ex: 1000
    /// </summary>
    [Column("vert_vis_ft")]
    public int? VertVisFt { get; set; }

    /// <summary>
    /// Type of encoding ex: METAR, SPECI, SYNOP, BUOY, CMAN
    /// </summary>
    [Column("metar_type")]
    public string? MetarType { get; set; }

    /// <summary>
    /// Elevation of site in meters ex: 202
    /// </summary>
    [Column("elevation_m")]
    public float? ElevationM { get; set; }
}