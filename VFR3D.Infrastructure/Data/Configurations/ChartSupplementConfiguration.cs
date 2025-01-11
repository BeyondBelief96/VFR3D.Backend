using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using VFR3D.Domain.Entities;

namespace VFR3D.Infrastructure.Data.Configurations
{
    public class ChartSupplementConfiguration : IEntityTypeConfiguration<ChartSupplement>
    {
        public void Configure(EntityTypeBuilder<ChartSupplement> builder)
        {
            builder.HasIndex(e => e.AirportCode);
            builder.HasIndex(e => e.NavigationalAidName);

            builder.Property(e => e.AirportName)
                .HasMaxLength(255);

            builder.Property(e => e.AirportCity)
                .HasMaxLength(255);

            builder.Property(e => e.AirportCode)
                .HasMaxLength(10);

            builder.Property(e => e.NavigationalAidName)
                .HasMaxLength(255);

            builder.Property(e => e.FileName)
                .HasMaxLength(255);
        }
    }
}
