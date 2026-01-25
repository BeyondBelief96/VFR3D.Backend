using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VFR3D.Domain.ValueObjects.GAirmets;

namespace VFR3D.Domain.Entities
{
    [Table("gairmet")]
    public class GAirmet
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Time the G-AIRMET was received
        /// </summary>
        [Column("receipt_time")]
        public DateTime ReceiptTime { get; set; }

        /// <summary>
        /// Time the G-AIRMET was issued
        /// </summary>
        [Column("issue_time")]
        public DateTime IssueTime { get; set; }

        /// <summary>
        /// Time the G-AIRMET expires
        /// </summary>
        [Column("expire_time")]
        public DateTime ExpireTime { get; set; }

        /// <summary>
        /// Time the G-AIRMET is valid for
        /// </summary>
        [Column("valid_time")]
        public DateTime ValidTime { get; set; }

        /// <summary>
        /// Product type: SIERRA, TANGO, or ZULU
        /// </summary>
        [Column("product")]
        public string Product { get; set; } = string.Empty;

        /// <summary>
        /// Forecast component tag
        /// </summary>
        [Column("tag")]
        public string? Tag { get; set; }

        /// <summary>
        /// Number of hours between issue time and valid time
        /// </summary>
        [Column("forecast_hour")]
        public int ForecastHour { get; set; }

        /// <summary>
        /// Type of hazard (e.g., MT_OBSC, TURB, ICE, IFR, etc.)
        /// </summary>
        [Column("hazard_type")]
        public string? HazardType { get; set; }

        /// <summary>
        /// Severity of the hazard
        /// </summary>
        [Column("hazard_severity")]
        public string? HazardSeverity { get; set; }

        /// <summary>
        /// Geometry type: AREA or LINE
        /// </summary>
        [Column("geometry_type")]
        public string? GeometryType { get; set; }

        /// <summary>
        /// Cause of the hazard (e.g., "MTNS OBSC BY PCPN/CLDS")
        /// </summary>
        [Column("due_to")]
        public string? DueTo { get; set; }

        /// <summary>
        /// Altitude information (can have multiple altitude ranges)
        /// </summary>
        [Column("altitudes", TypeName = "jsonb")]
        public List<GAirmetAltitude>? Altitudes { get; set; }

        /// <summary>
        /// The geographic area defined by lat/lon points
        /// </summary>
        [Column("area", TypeName = "jsonb")]
        public GAirmetArea? Area { get; set; }
    }
}
