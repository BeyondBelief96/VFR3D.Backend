using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using VFR3D.Domain.ValueObjects.Airsigmets;

namespace VFR3D.Domain.Entities
{
    [Table("airsigmet")]
    public class Airsigmet
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Raw text of AIRSIGMET
        /// </summary>
        [Column("raw_text")]
        public string? RawText { get; set; }

        /// <summary>
        /// The time the advisory starts
        /// </summary>
        [Column("valid_time_from")]
        public string? ValidTimeFrom { get; set; }

        /// <summary>
        /// The time the advisory ends
        /// </summary>
        [Column("valid_time_to")]
        public string? ValidTimeTo { get; set; }

        /// <summary>
        /// The bottom and/or top levels the product is valid for in feet above mean sea level
        /// </summary>
        [Column("altitude", TypeName = "jsonb")]
        public AirsigmetAltitude? Altitude { get; set; }

        /// <summary>
        /// The movement direction of the hazard area in degrees
        /// </summary>
        [Column("movement_dir_degrees")]
        public int? MovementDirDegrees { get; set; }

        /// <summary>
        /// The movement speed of the hazard area in knots
        /// </summary>
        [Column("movement_speed_kt")]
        public int? MovementSpeedKt { get; set; }

        /// <summary>
        /// The hazard type and severity
        /// </summary>
        [Column("hazard", TypeName = "jsonb")]
        public AirsigmetHazard? Hazard { get; set; }

        /// <summary>
        /// The type of product: SIGMET, AIRMET, OUTLOOK
        /// </summary>
        [Column("airsigmet_type")]
        public string? AirsigmetType { get; set; }

        /// <summary>
        /// The area or line given in latitude, longitude points
        /// </summary>
        [Column("area", TypeName = "jsonb")]
        public List<AirsigmetArea>? Areas { get; set; }
    }
}
