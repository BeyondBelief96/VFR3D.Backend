using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace VFR3D.Domain.Entities
{
    [Table("communication_frequencies")]
    public class CommunicationFrequency : INasrEntity<CommunicationFrequency>
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

        // INasrEntity<CommunicationFrequency> implementation
        public string CreateUniqueKey()
        {
            return string.Join("|", new[]
            {
                FacilityCode ?? string.Empty,
                ServicedFacility ?? string.Empty,
                ServicedSiteType ?? string.Empty,
                ServicedState ?? string.Empty,
                Frequency ?? string.Empty,
                FrequencyUse ?? string.Empty,
                Sectorization ?? string.Empty
            });
        }

        public void UpdateFrom(CommunicationFrequency source, HashSet<string>? limitToProperties = null)
        {
            // For standalone entities like CommunicationFrequency, we typically update all properties
            // since there's no supplementary data concept
            if (limitToProperties == null || !limitToProperties.Any())
            {
                UpdateAllProperties(source);
            }
            else
            {
                UpdateSelectiveProperties(source, limitToProperties);
            }
        }

        public CommunicationFrequency CreateSelectiveEntity(HashSet<string> properties)
        {
            var selective = new CommunicationFrequency();

            // Key properties
            if (properties.Contains(nameof(FacilityCode)))
                selective.FacilityCode = FacilityCode;
            if (properties.Contains(nameof(ServicedFacility)))
                selective.ServicedFacility = ServicedFacility;
            if (properties.Contains(nameof(ServicedSiteType)))
                selective.ServicedSiteType = ServicedSiteType;
            if (properties.Contains(nameof(ServicedState)))
                selective.ServicedState = ServicedState;
            if (properties.Contains(nameof(Frequency)))
                selective.Frequency = Frequency;
            if (properties.Contains(nameof(FrequencyUse)))
                selective.FrequencyUse = FrequencyUse;
            if (properties.Contains(nameof(Sectorization)))
                selective.Sectorization = Sectorization;

            // Other properties
            if (properties.Contains(nameof(EffectiveDate)))
                selective.EffectiveDate = EffectiveDate;
            if (properties.Contains(nameof(FacilityName)))
                selective.FacilityName = FacilityName;
            if (properties.Contains(nameof(FacilityType)))
                selective.FacilityType = FacilityType;
            if (properties.Contains(nameof(ArtccOrFssId)))
                selective.ArtccOrFssId = ArtccOrFssId;
            if (properties.Contains(nameof(Cpdlc)))
                selective.Cpdlc = Cpdlc;
            if (properties.Contains(nameof(TowerHours)))
                selective.TowerHours = TowerHours;
            if (properties.Contains(nameof(ServicedFacilityName)))
                selective.ServicedFacilityName = ServicedFacilityName;
            if (properties.Contains(nameof(Latitude)))
                selective.Latitude = Latitude;
            if (properties.Contains(nameof(Longitude)))
                selective.Longitude = Longitude;
            if (properties.Contains(nameof(ServicedCity)))
                selective.ServicedCity = ServicedCity;
            if (properties.Contains(nameof(ServicedCountry)))
                selective.ServicedCountry = ServicedCountry;
            if (properties.Contains(nameof(TowerOrCommCall)))
                selective.TowerOrCommCall = TowerOrCommCall;
            if (properties.Contains(nameof(PrimaryApproachRadioCall)))
                selective.PrimaryApproachRadioCall = PrimaryApproachRadioCall;
            if (properties.Contains(nameof(Remark)))
                selective.Remark = Remark;

            return selective;
        }

        private void UpdateAllProperties(CommunicationFrequency source)
        {
            EffectiveDate = source.EffectiveDate;
            FacilityName = source.FacilityName;
            FacilityType = source.FacilityType;
            ArtccOrFssId = source.ArtccOrFssId;
            Cpdlc = source.Cpdlc;
            TowerHours = source.TowerHours;
            ServicedFacilityName = source.ServicedFacilityName;
            Latitude = source.Latitude;
            Longitude = source.Longitude;
            ServicedCity = source.ServicedCity;
            ServicedCountry = source.ServicedCountry;
            TowerOrCommCall = source.TowerOrCommCall;
            PrimaryApproachRadioCall = source.PrimaryApproachRadioCall;
            Remark = source.Remark;
        }

        private void UpdateSelectiveProperties(CommunicationFrequency source, HashSet<string> limitToProperties)
        {
            // Update only specified properties with non-null values
            if (limitToProperties.Contains(nameof(EffectiveDate)))
                EffectiveDate = source.EffectiveDate;
            if (limitToProperties.Contains(nameof(FacilityName)) && source.FacilityName != null)
                FacilityName = source.FacilityName;
            if (limitToProperties.Contains(nameof(FacilityType)) && source.FacilityType != null)
                FacilityType = source.FacilityType;
            if (limitToProperties.Contains(nameof(ArtccOrFssId)) && source.ArtccOrFssId != null)
                ArtccOrFssId = source.ArtccOrFssId;
            if (limitToProperties.Contains(nameof(Cpdlc)) && source.Cpdlc != null)
                Cpdlc = source.Cpdlc;
            if (limitToProperties.Contains(nameof(TowerHours)) && source.TowerHours != null)
                TowerHours = source.TowerHours;
            if (limitToProperties.Contains(nameof(ServicedFacilityName)) && source.ServicedFacilityName != null)
                ServicedFacilityName = source.ServicedFacilityName;
            if (limitToProperties.Contains(nameof(Latitude)) && source.Latitude != null)
                Latitude = source.Latitude;
            if (limitToProperties.Contains(nameof(Longitude)) && source.Longitude != null)
                Longitude = source.Longitude;
            if (limitToProperties.Contains(nameof(ServicedCity)) && source.ServicedCity != null)
                ServicedCity = source.ServicedCity;
            if (limitToProperties.Contains(nameof(ServicedCountry)) && source.ServicedCountry != null)
                ServicedCountry = source.ServicedCountry;
            if (limitToProperties.Contains(nameof(TowerOrCommCall)) && source.TowerOrCommCall != null)
                TowerOrCommCall = source.TowerOrCommCall;
            if (limitToProperties.Contains(nameof(PrimaryApproachRadioCall)) && source.PrimaryApproachRadioCall != null)
                PrimaryApproachRadioCall = source.PrimaryApproachRadioCall;
            if (limitToProperties.Contains(nameof(Remark)) && source.Remark != null)
                Remark = source.Remark;
        }
    }
}
