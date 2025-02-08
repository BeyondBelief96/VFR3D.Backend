using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VFR3D.Domain.Entities;

namespace VFR3D.Infrastructure.Data.Configurations
{
    public class CommunicationFrequencyConfiguration : IEntityTypeConfiguration<CommunicationFrequency>
    {
        public void Configure(EntityTypeBuilder<CommunicationFrequency> builder)
        {
            builder.ToTable("communication_frequencies");

            // Primary Key
            builder.HasKey(e => e.Id);

            // Required fields
            builder.Property(e => e.FacilityType)
                .IsRequired()
                .HasMaxLength(12);

            builder.Property(e => e.ServicedFacility)
                .IsRequired()
                .HasMaxLength(30);

            // Optional fields with max lengths
            builder.Property(e => e.FacilityCode)
                .HasMaxLength(30);

            builder.Property(e => e.FacilityName)
                .HasMaxLength(50);

            builder.Property(e => e.ArtccOrFssId)
                .HasMaxLength(4);

            builder.Property(e => e.Cpdlc)
                .HasMaxLength(100);

            builder.Property(e => e.TowerHours)
                .HasMaxLength(200);

            builder.Property(e => e.ServicedFacilityName)
                .HasMaxLength(50);

            builder.Property(e => e.ServicedSiteType)
                .HasMaxLength(25);

            builder.Property(e => e.ServicedCity)
                .HasMaxLength(40);

            builder.Property(e => e.ServicedState)
                .HasMaxLength(2);

            builder.Property(e => e.ServicedCountry)
                .HasMaxLength(2);

            builder.Property(e => e.TowerOrCommCall)
                .HasMaxLength(30);

            builder.Property(e => e.PrimaryApproachRadioCall)
                .HasMaxLength(26);

            builder.Property(e => e.Frequency)
                .HasMaxLength(40);

            builder.Property(e => e.Sectorization)
                .HasMaxLength(50);

            builder.Property(e => e.FrequencyUse)
                .HasMaxLength(600);

            builder.Property(e => e.Remark)
                .HasMaxLength(1500);

            // Decimal precision for coordinates
            builder.Property(e => e.Latitude)
                .HasColumnType("decimal(10,8)");

            builder.Property(e => e.Longitude)
                .HasColumnType("decimal(11,8)");

            // Indexes for lookups
            builder.HasIndex(e => e.FacilityCode);
            builder.HasIndex(e => e.ServicedFacility);
            builder.HasIndex(e => e.ServicedState);
            builder.HasIndex(
                    e => new { e.FacilityCode, e.ServicedFacility, e.ServicedSiteType, e.ServicedState, e.Frequency, e.FrequencyUse, e.Sectorization }
                )
                .IsUnique()
                .HasFilter("\"facility_code\" IS NOT NULL AND \"serviced_facility\" IS NOT NULL AND \"serviced_site_type\" IS NOT NULL AND \"serviced_state\" IS NOT NULL AND \"frequency\" IS NOT NULL AND \"frequency_use\" IS NOT NULL AND \"sectorization\" IS NOT NULL");

            // Column type specifications
            builder.Property(e => e.EffectiveDate)
                .HasColumnType("date");
        }
    }
}