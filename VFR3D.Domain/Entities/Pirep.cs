using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using VFR3D.Domain.ValueObjects.Pireps;

namespace VFR3D.Domain.Entities
{
    [Table("pirep")]
    public class Pirep
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// The time the observation was received (ISO 8601 date format)
        /// </summary>
        [Column("receipt_time")]
        public string? ReceiptTime { get; set; }

        /// <summary>
        /// The observation time (ISO 8601 date format)
        /// </summary>
        [Column("observation_time")]
        public string? ObservationTime { get; set; }

        [Column("quality_control_flags", TypeName = "jsonb")]
        public PirepQualityControlFlags? QualityControlFlags { get; set; }

        [Column("aircraft_ref")]
        public string? AircraftRef { get; set; }

        [Column("latitude")]
        public float? Latitude { get; set; }

        [Column("longitude")]
        public float? Longitude { get; set; }

        [Column("altitude_ft_msl")]
        public int? AltitudeFtMsl { get; set; }

        [Column("sky_condition", TypeName = "jsonb")]
        public List<PirepSkyCondition>? SkyConditions { get; set; }

        [Column("turbulence_condition", TypeName = "jsonb")]
        public List<PirepTurbulenceCondition>? TurbulenceConditions { get; set; }

        [Column("icing_condition", TypeName = "jsonb")]
        public List<PirepIcingCondition>? IcingConditions { get; set; }

        [Column("visibility_statute_mi")]
        public int? VisibilityStatuteMi { get; set; }

        [Column("wx_string")]
        public string? WxString { get; set; }

        [Column("temp_c")]
        public float? TempC { get; set; }

        [Column("wind_dir_degrees")]
        public int? WindDirDegrees { get; set; }

        [Column("wind_speed_kt")]
        public int? WindSpeedKt { get; set; }

        [Column("vert_gust_kt")]
        public int? VertGustKt { get; set; }

        /// <summary>
        /// Report type ex: PIREP, Urgent PIREP, AIREP, AMDAR
        /// </summary>
        [Column("report_type")]
        public string? ReportType { get; set; }

        /// <summary>
        /// Raw text of observation
        /// </summary>
        [Column("raw_text")]
        public string? RawText { get; set; }
    }
}
