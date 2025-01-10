using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using VFR3D.Domain.ValueObjects.Taf;

namespace VFR3D.Domain.Entities
{
    [Table("taf")]
    public class Taf
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Raw text of forecast ex: KORD 032151Z 23006KT 10SM BKN110 OVC250 14/03 A3000 RMK AO2 SLP162 VIRGA OHD T01440028
        /// </summary>
        [Column("raw_text")]
        public string? RawText { get; set; }

        /// <summary>
        /// ICAO identifier ex: KORD
        /// </summary>
        [Column("station_id")]
        [MaxLength(4)]
        public string? StationId { get; set; }

        /// <summary>
        /// The time the product was issued (ISO 8601 date format) ex: 2023-11-04T08:50:00Z
        /// </summary>
        [Column("issue_time")]
        public string? IssueTime { get; set; }

        /// <summary>
        /// The official time of the bulletin (ISO 8601 date format) ex: 2023-11-04T08:50:00Z
        /// </summary>
        [Column("bulletin_time")]
        public string? BulletinTime { get; set; }

        /// <summary>
        /// The time the period of validity starts (ISO 8601 date format) ex: 2023-11-03T21:00:00Z
        /// </summary>
        [Column("valid_time_from")]
        public string? ValidTimeFrom { get; set; }

        /// <summary>
        /// The time the period of validity ends (ISO 8601 date format) ex: 2023-11-05T03:00:00Z
        /// </summary>
        [Column("valid_time_to")]
        public string? ValidTimeTo { get; set; }

        /// <summary>
        /// Any additional remarks for TAF ex: RMK PCG
        /// </summary>
        [Column("remarks")]
        public string? Remarks { get; set; }

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
        /// Elevation of site in meters ex: 202
        /// </summary>
        [Column("elevation_m")]
        public float? ElevationM { get; set; }

        /// <summary>
        /// Collection of forecast periods within the TAF
        /// </summary>
        [Column("forecast", TypeName = "jsonb")]
        public List<TafForecast>? Forecast { get; set; }
    }
}
