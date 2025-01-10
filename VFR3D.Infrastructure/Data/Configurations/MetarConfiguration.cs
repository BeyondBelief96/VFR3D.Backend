using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;
using Vfr3d.Domain.Entities;
using VFR3D.Domain.ValueObjects.Metar;

namespace VFR3D.Infrastructure.Data.Configurations
{
    public class MetarConfiguration : IEntityTypeConfiguration<Metar>
    {
        public void Configure(EntityTypeBuilder<Metar> builder)
        {
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };

            // String length constraints
            builder.Property(e => e.StationId).HasMaxLength(4);
            builder.Property(e => e.WindDirDegrees).HasMaxLength(3);
            builder.Property(e => e.FlightCategory).HasMaxLength(4);
            builder.Property(e => e.MetarType).HasMaxLength(5);

            // Indexes for performance
            builder.HasIndex(e => e.StationId);
            builder.HasIndex(e => e.ObservationTime);
            // Add composite index for station and observation time
            builder.HasIndex(e => new { e.StationId, e.ObservationTime });

            // JSON conversions
            builder.Property(x => x.QualityControlFlags)
            .HasConversion(
                v => JsonSerializer.Serialize(v, jsonOptions),
                v => JsonSerializer.Deserialize<MetarQualityControlFlags>(v ?? "{}", jsonOptions),
                new ValueComparer<MetarQualityControlFlags>(
                    (l, r) => JsonSerializer.Serialize(l, jsonOptions) == JsonSerializer.Serialize(r, jsonOptions),
                    v => v == null ? 0 : JsonSerializer.Serialize(v, jsonOptions).GetHashCode(),
                    v => JsonSerializer.Deserialize<MetarQualityControlFlags>(JsonSerializer.Serialize(v, jsonOptions), jsonOptions)
                ));

            builder.Property(x => x.SkyCondition)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<List<MetarSkyCondition>>(v ?? "[]", jsonOptions),
                    new ValueComparer<List<MetarSkyCondition>>(
                        (l, r) => JsonSerializer.Serialize(l, jsonOptions) == JsonSerializer.Serialize(r, jsonOptions),
                        v => v == null ? 0 : JsonSerializer.Serialize(v, jsonOptions).GetHashCode(),
                        v => JsonSerializer.Deserialize<List<MetarSkyCondition>>(JsonSerializer.Serialize(v, jsonOptions), jsonOptions)
                    ));
        }
    }
}
