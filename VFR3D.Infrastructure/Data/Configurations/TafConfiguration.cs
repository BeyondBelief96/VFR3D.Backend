using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using VFR3D.Domain.Entities;
using VFR3D.Domain.ValueObjects.Taf;

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
                    v => v == null ? "[]" : JsonSerializer.Serialize(v, jsonOptions),
                    v => string.IsNullOrEmpty(v) ? new List<TafForecast>() : JsonSerializer.Deserialize<List<TafForecast>>(v, jsonOptions) ?? new List<TafForecast>(),
                    new ValueComparer<List<TafForecast>>(
                        (l, r) => (l == null && r == null) || (l != null && r != null && JsonSerializer.Serialize(l, jsonOptions) == JsonSerializer.Serialize(r, jsonOptions)),
                        v => v == null ? 0 : JsonSerializer.Serialize(v, jsonOptions).GetHashCode(),
                        v => v == null ? new List<TafForecast>() : JsonSerializer.Deserialize<List<TafForecast>>(JsonSerializer.Serialize(v, jsonOptions), jsonOptions) ?? new List<TafForecast>()
                    ));
        }
    }
}