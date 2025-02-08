using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace VFR3D.Domain.Entities
{
    [Table("communication_frequencies")]
    public class CommunicationFrequency
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Column("facility_code", TypeName = "varchar(30)")]
        public string? FacilityCode { get; set; }

        [Column("effective_date")]
        public DateTime EffectiveDate { get; set; }

        [Column("facility_name", TypeName = "varchar(50)")]
        public string? FacilityName { get; set; }

        [Column("facility_type", TypeName = "varchar(12)")]
        [Required]
        public string FacilityType { get; set; } = string.Empty;

        [Column("artcc_or_fss_id", TypeName = "varchar(4)")]
        public string? ArtccOrFssId { get; set; }

        [Column("cpdlc", TypeName = "varchar(100)")]
        public string? Cpdlc { get; set; }

        [Column("tower_hours", TypeName = "varchar(200)")]
        public string? TowerHours { get; set; }

        [Column("serviced_facility", TypeName = "varchar(30)")]
        [Required]
        public string ServicedFacility { get; set; } = string.Empty;

        [Column("serviced_facility_name", TypeName = "varchar(50)")]
        public string? ServicedFacilityName { get; set; }

        [Column("serviced_site_type", TypeName = "varchar(25)")]
        public string? ServicedSiteType { get; set; }

        [Column("latitude", TypeName = "decimal(10,8)")]
        public decimal? Latitude { get; set; }

        [Column("longitude", TypeName = "decimal(11,8)")]
        public decimal? Longitude { get; set; }

        [Column("serviced_city", TypeName = "varchar(40)")]
        public string? ServicedCity { get; set; }

        [Column("serviced_state", TypeName = "varchar(2)")]
        public string? ServicedState { get; set; }

        [Column("serviced_country", TypeName = "varchar(2)")]
        public string? ServicedCountry { get; set; }

        [Column("tower_or_comm_call", TypeName = "varchar(30)")]
        public string? TowerOrCommCall { get; set; }

        [Column("primary_approach_radio_call", TypeName = "varchar(26)")]
        public string? PrimaryApproachRadioCall { get; set; }

        [Column("frequency", TypeName = "varchar(40)")]
        public string? Frequency { get; set; }

        [Column("sectorization", TypeName = "varchar(50)")]
        public string? Sectorization { get; set; }

        [Column("frequency_use", TypeName = "varchar(600)")]
        public string? FrequencyUse { get; set; }

        [Column("remark", TypeName = "varchar(1500)")]
        public string? Remark { get; set; }
    }
}
