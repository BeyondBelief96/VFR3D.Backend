using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using VFR3D.Domain.Entities;

namespace VFR3D.Infrastructure.Data.Configurations
{
    public class AirportDiagramConfiguration : IEntityTypeConfiguration<AirportDiagram>
    {
        public void Configure(EntityTypeBuilder<AirportDiagram> builder)
        {
            builder.ToTable("airport_diagrams");

            builder.Property(e => e.Id)
                .HasColumnName("id");

            builder.Property(e => e.AirportName)
                .HasColumnName("airport_name")
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.IcaoIdent)
                .HasColumnName("icao_ident")
                .HasMaxLength(4);

            builder.Property(e => e.AirportIdent)
                .HasColumnName("airport_ident")
                .HasMaxLength(4);

            builder.Property(e => e.FileName)
                .HasColumnName("file_name")
                .HasMaxLength(100);

            // Add indexes
            builder.HasIndex(e => e.IcaoIdent);
            builder.HasIndex(e => e.AirportIdent);
        }
    }
}
