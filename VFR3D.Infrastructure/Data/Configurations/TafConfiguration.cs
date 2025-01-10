using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using VFR3D.Domain.Entities;
using VFR3D.Domain.ValueObjects.Taf;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace VFR3D.Infrastructure.Data.Configurations
{
    public class TafConfiguration : IEntityTypeConfiguration<Taf>
    {
        public void Configure(EntityTypeBuilder<Taf> builder)
        {
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };

            builder.Property(e => e.StationId).HasMaxLength(4);

            builder.HasIndex(e => e.StationId);
            builder.HasIndex(e => e.ValidTimeFrom);
            builder.HasIndex(e => new { e.StationId, e.ValidTimeFrom });

            builder.Property(x => x.Forecast)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<List<TafForecast>>(v ?? "[]", jsonOptions),
                    new ValueComparer<List<TafForecast>>(
                        (l, r) => JsonSerializer.Serialize(l, jsonOptions) == JsonSerializer.Serialize(r, jsonOptions),
                        v => v == null ? 0 : JsonSerializer.Serialize(v, jsonOptions).GetHashCode(),
                        v => JsonSerializer.Deserialize<List<TafForecast>>(JsonSerializer.Serialize(v, jsonOptions), jsonOptions)
                    ));
        }
    }
}
