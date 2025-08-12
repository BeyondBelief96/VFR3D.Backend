using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using VFR3D.Domain.Entities;

namespace VFR3D.Infrastructure.Data.Configurations
{
    public class AirspaceConfiguration : IEntityTypeConfiguration<Airspace>
    {
        public void Configure(EntityTypeBuilder<Airspace> builder)
        {
            builder.ToTable("airspaces");

            builder.HasKey(e => e.GlobalId);

            builder.Property(e => e.GlobalId)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(e => e.Ident)
                .HasMaxLength(200);

            builder.Property(e => e.IcaoId)
                .HasMaxLength(200);

            builder.Property(e => e.Name)
                .HasMaxLength(200);

            builder.Property(e => e.UpperDesc)
                .HasMaxLength(200);

            builder.Property(e => e.UpperUom)
                .HasMaxLength(200);

            builder.Property(e => e.UpperCode)
                .HasMaxLength(200);

            builder.Property(e => e.LowerDesc)
                .HasMaxLength(200);

            builder.Property(e => e.LowerUom)
                .HasMaxLength(200);

            builder.Property(e => e.LowerCode)
                .HasMaxLength(200);

            builder.Property(e => e.TypeCode)
                .HasMaxLength(200);

            builder.Property(e => e.LocalType)
                .HasMaxLength(200);

            builder.Property(e => e.Class)
                .HasMaxLength(200);

            builder.Property(e => e.MilCode)
                .HasMaxLength(200);

            builder.Property(e => e.CommName)
                .HasMaxLength(200);

            builder.Property(e => e.Level)
                .HasMaxLength(200);

            builder.Property(e => e.Sector)
                .HasMaxLength(200);

            builder.Property(e => e.Onshore)
                .HasMaxLength(200);

            builder.Property(e => e.Exclusion)
                .HasMaxLength(200);

            builder.Property(e => e.WkhrCode)
                .HasMaxLength(200);

            builder.Property(e => e.WkhrRmk)
                .HasMaxLength(200);

            builder.Property(e => e.Dst)
                .HasMaxLength(200);

            builder.Property(e => e.GmtOffset)
                .HasMaxLength(200);

            builder.Property(e => e.ContAgent)
                .HasMaxLength(254);

            builder.Property(e => e.City)
                .HasMaxLength(254);

            builder.Property(e => e.State)
                .HasMaxLength(254);

            builder.Property(e => e.Country)
                .HasMaxLength(254);

            builder.Property(e => e.AdhpId)
                .HasMaxLength(50);

            builder.Property(e => e.Geometry)
                .HasColumnType("geometry(Polygon, 4326)");

            // Indexes
            builder.HasIndex(e => e.GlobalId).IsUnique();
            builder.HasIndex(e => e.IcaoId);
            builder.HasIndex(e => e.Name);
            builder.HasIndex(e => e.State);
            builder.HasIndex(e => e.Class);
        }
    }
}
