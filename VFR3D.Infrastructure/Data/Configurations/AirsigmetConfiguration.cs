using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using VFR3D.Domain.Entities;
using VFR3D.Domain.ValueObjects.Airsigmets;

namespace VFR3D.Infrastructure.Data.Configurations
{
    public class AirsigmetConfiguration : IEntityTypeConfiguration<Airsigmet>
    {
        public void Configure(EntityTypeBuilder<Airsigmet> builder)
        {
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };

            builder.HasIndex(e => e.ValidTimeFrom);
            builder.HasIndex(e => e.ValidTimeTo);
            builder.HasIndex(e => e.AirsigmetType);

            builder.Property(x => x.Altitude)
                .HasConversion(
                    v => v == null ? "{}" : JsonSerializer.Serialize(v, jsonOptions),
                    v => string.IsNullOrEmpty(v) ? new AirsigmetAltitude() : JsonSerializer.Deserialize<AirsigmetAltitude>(v, jsonOptions) ?? new AirsigmetAltitude(),
                    new ValueComparer<AirsigmetAltitude>(
                        (l, r) => (l == null && r == null) || (l != null && r != null && JsonSerializer.Serialize(l, jsonOptions) == JsonSerializer.Serialize(r, jsonOptions)),
                        v => v == null ? 0 : JsonSerializer.Serialize(v, jsonOptions).GetHashCode(),
                        v => v == null ? new AirsigmetAltitude() : JsonSerializer.Deserialize<AirsigmetAltitude>(JsonSerializer.Serialize(v, jsonOptions), jsonOptions) ?? new AirsigmetAltitude()
                    ));

            builder.Property(x => x.Hazard)
                .HasConversion(
                    v => v == null ? "{}" : JsonSerializer.Serialize(v, jsonOptions),
                    v => string.IsNullOrEmpty(v) ? new AirsigmetHazard() : JsonSerializer.Deserialize<AirsigmetHazard>(v, jsonOptions) ?? new AirsigmetHazard(),
                    new ValueComparer<AirsigmetHazard>(
                        (l, r) => (l == null && r == null) || (l != null && r != null && JsonSerializer.Serialize(l, jsonOptions) == JsonSerializer.Serialize(r, jsonOptions)),
                        v => v == null ? 0 : JsonSerializer.Serialize(v, jsonOptions).GetHashCode(),
                        v => v == null ? new AirsigmetHazard() : JsonSerializer.Deserialize<AirsigmetHazard>(JsonSerializer.Serialize(v, jsonOptions), jsonOptions) ?? new AirsigmetHazard()
                    ));

            builder.Property(x => x.Areas)
                .HasConversion(
                    v => v == null ? "[]" : JsonSerializer.Serialize(v, jsonOptions),
                    v => string.IsNullOrEmpty(v) ? new List<AirsigmetArea>() : JsonSerializer.Deserialize<List<AirsigmetArea>>(v, jsonOptions) ?? new List<AirsigmetArea>(),
                    new ValueComparer<List<AirsigmetArea>>(
                        (l, r) => (l == null && r == null) || (l != null && r != null && JsonSerializer.Serialize(l, jsonOptions) == JsonSerializer.Serialize(r, jsonOptions)),
                        v => v == null ? 0 : JsonSerializer.Serialize(v, jsonOptions).GetHashCode(),
                        v => v == null ? new List<AirsigmetArea>() : JsonSerializer.Deserialize<List<AirsigmetArea>>(JsonSerializer.Serialize(v, jsonOptions), jsonOptions) ?? new List<AirsigmetArea>()
                    ));
        }
    }
}