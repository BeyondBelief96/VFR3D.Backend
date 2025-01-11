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

            // Indexes for querying
            builder.HasIndex(e => e.ValidTimeFrom);
            builder.HasIndex(e => e.ValidTimeTo);
            builder.HasIndex(e => e.AirsigmetType);

            // Configure JSON conversions
            builder.Property(x => x.Altitude)
                .HasConversion(
                    v => JsonSerializer.Serialize(v ?? new AirsigmetAltitude(), jsonOptions),
                    v => JsonSerializer.Deserialize<AirsigmetAltitude>(v ?? "{}", jsonOptions),
                    new ValueComparer<AirsigmetAltitude>(
                        (l, r) => JsonSerializer.Serialize(l, jsonOptions) == JsonSerializer.Serialize(r, jsonOptions),
                        v => v == null ? 0 : JsonSerializer.Serialize(v, jsonOptions).GetHashCode(),
                        v => JsonSerializer.Deserialize<AirsigmetAltitude>(
                            JsonSerializer.Serialize(v ?? new AirsigmetAltitude(), jsonOptions),
                            jsonOptions)
                    ));

            builder.Property(x => x.Hazard)
                .HasConversion(
                    v => JsonSerializer.Serialize(v ?? new AirsigmetHazard(), jsonOptions),
                    v => JsonSerializer.Deserialize<AirsigmetHazard>(v ?? "{}", jsonOptions),
                    new ValueComparer<AirsigmetHazard>(
                        (l, r) => JsonSerializer.Serialize(l, jsonOptions) == JsonSerializer.Serialize(r, jsonOptions),
                        v => v == null ? 0 : JsonSerializer.Serialize(v, jsonOptions).GetHashCode(),
                        v => JsonSerializer.Deserialize<AirsigmetHazard>(
                            JsonSerializer.Serialize(v ?? new AirsigmetHazard(), jsonOptions),
                            jsonOptions)
                    ));

            builder.Property(x => x.Areas)
                .HasConversion(
                    v => JsonSerializer.Serialize(v ?? new List<AirsigmetArea>(), jsonOptions),
                    v => JsonSerializer.Deserialize<List<AirsigmetArea>>(v ?? "[]", jsonOptions),
                    new ValueComparer<List<AirsigmetArea>>(
                        (l, r) => JsonSerializer.Serialize(l, jsonOptions) == JsonSerializer.Serialize(r, jsonOptions),
                        v => v == null ? 0 : JsonSerializer.Serialize(v, jsonOptions).GetHashCode(),
                        v => JsonSerializer.Deserialize<List<AirsigmetArea>>(
                            JsonSerializer.Serialize(v ?? new List<AirsigmetArea>(), jsonOptions),
                            jsonOptions)
                    ));
        }
    }
}
