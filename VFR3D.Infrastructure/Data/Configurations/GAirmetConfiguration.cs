using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;
using VFR3D.Domain.Entities;
using VFR3D.Domain.ValueObjects.GAirmets;

namespace VFR3D.Infrastructure.Data.Configurations
{
    public class GAirmetConfiguration : IEntityTypeConfiguration<GAirmet>
    {
        public void Configure(EntityTypeBuilder<GAirmet> builder)
        {
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };

            builder.HasIndex(e => e.IssueTime);
            builder.HasIndex(e => e.ExpireTime);
            builder.HasIndex(e => e.ValidTime);
            builder.HasIndex(e => e.Product);
            builder.HasIndex(e => e.HazardType);

            builder.Property(x => x.Area)
                .HasConversion(
                    v => v == null ? "{}" : JsonSerializer.Serialize(v, jsonOptions),
                    v => string.IsNullOrEmpty(v) ? new GAirmetArea() : JsonSerializer.Deserialize<GAirmetArea>(v, jsonOptions) ?? new GAirmetArea(),
                    new ValueComparer<GAirmetArea>(
                        (l, r) => (l == null && r == null) || (l != null && r != null && JsonSerializer.Serialize(l, jsonOptions) == JsonSerializer.Serialize(r, jsonOptions)),
                        v => v == null ? 0 : JsonSerializer.Serialize(v, jsonOptions).GetHashCode(),
                        v => v == null ? new GAirmetArea() : JsonSerializer.Deserialize<GAirmetArea>(JsonSerializer.Serialize(v, jsonOptions), jsonOptions) ?? new GAirmetArea()
                    ));

            builder.Property(x => x.Altitudes)
                .HasConversion(
                    v => v == null ? "[]" : JsonSerializer.Serialize(v, jsonOptions),
                    v => string.IsNullOrEmpty(v) ? new List<GAirmetAltitude>() : JsonSerializer.Deserialize<List<GAirmetAltitude>>(v, jsonOptions) ?? new List<GAirmetAltitude>(),
                    new ValueComparer<List<GAirmetAltitude>>(
                        (l, r) => (l == null && r == null) || (l != null && r != null && JsonSerializer.Serialize(l, jsonOptions) == JsonSerializer.Serialize(r, jsonOptions)),
                        v => v == null ? 0 : JsonSerializer.Serialize(v, jsonOptions).GetHashCode(),
                        v => v == null ? new List<GAirmetAltitude>() : JsonSerializer.Deserialize<List<GAirmetAltitude>>(JsonSerializer.Serialize(v, jsonOptions), jsonOptions) ?? new List<GAirmetAltitude>()
                    ));
        }
    }
}
