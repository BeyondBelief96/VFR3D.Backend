using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using VFR3D.Domain.Entities;
using VFR3D.Domain.ValueObjects.Pireps;

namespace VFR3D.Infrastructure.Data.Configurations
{
    public class PirepConfiguration : IEntityTypeConfiguration<Pirep>
    {
        public void Configure(EntityTypeBuilder<Pirep> builder)
        {
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };

            // Indexes for better query performance
            builder.HasIndex(e => e.ObservationTime);
            builder.HasIndex(e => e.ReceiptTime);
            builder.HasIndex(e => new { e.Latitude, e.Longitude });

            // JSON conversions with value comparers
            builder.Property(x => x.QualityControlFlags)
                .HasConversion(
                    v => JsonSerializer.Serialize(v ?? new PirepQualityControlFlags(), jsonOptions),
                    v => JsonSerializer.Deserialize<PirepQualityControlFlags>(v ?? "{}", jsonOptions),
                    new ValueComparer<PirepQualityControlFlags>(
                        (l, r) => l == null && r == null ||
                                 (l != null && r != null &&
                                  JsonSerializer.Serialize(l, jsonOptions) == JsonSerializer.Serialize(r, jsonOptions)),
                        v => v == null ? 0 : JsonSerializer.Serialize(v, jsonOptions).GetHashCode(),
                        v => v == null ? null :
                             JsonSerializer.Deserialize<PirepQualityControlFlags>(
                                 JsonSerializer.Serialize(v ?? new PirepQualityControlFlags(), jsonOptions),
                                 jsonOptions)
                    ));

            builder.Property(x => x.SkyConditions)
                .HasConversion(
                    v => JsonSerializer.Serialize(v ?? new List<PirepSkyCondition>(), jsonOptions),
                    v => JsonSerializer.Deserialize<List<PirepSkyCondition>>(v ?? "[]", jsonOptions),
                    new ValueComparer<List<PirepSkyCondition>>(
                        (l, r) => l == null && r == null ||
                                 (l != null && r != null &&
                                  JsonSerializer.Serialize(l, jsonOptions) == JsonSerializer.Serialize(r, jsonOptions)),
                        v => v == null ? 0 : JsonSerializer.Serialize(v, jsonOptions).GetHashCode(),
                        v => v == null ? null :
                             JsonSerializer.Deserialize<List<PirepSkyCondition>>(
                                 JsonSerializer.Serialize(v ?? new List<PirepSkyCondition>(), jsonOptions),
                                 jsonOptions)
                    ));

            builder.Property(x => x.TurbulenceConditions)
                .HasConversion(
                    v => JsonSerializer.Serialize(v ?? new List<PirepTurbulenceCondition>(), jsonOptions),
                    v => JsonSerializer.Deserialize<List<PirepTurbulenceCondition>>(v ?? "[]", jsonOptions),
                    new ValueComparer<List<PirepTurbulenceCondition>>(
                        (l, r) => l == null && r == null ||
                                 (l != null && r != null &&
                                  JsonSerializer.Serialize(l, jsonOptions) == JsonSerializer.Serialize(r, jsonOptions)),
                        v => v == null ? 0 : JsonSerializer.Serialize(v, jsonOptions).GetHashCode(),
                        v => v == null ? null :
                             JsonSerializer.Deserialize<List<PirepTurbulenceCondition>>(
                                 JsonSerializer.Serialize(v ?? new List<PirepTurbulenceCondition>(), jsonOptions),
                                 jsonOptions)
                    ));

            builder.Property(x => x.IcingConditions)
                .HasConversion(
                    v => JsonSerializer.Serialize(v ?? new List<PirepIcingCondition>(), jsonOptions),
                    v => JsonSerializer.Deserialize<List<PirepIcingCondition>>(v ?? "[]", jsonOptions),
                    new ValueComparer<List<PirepIcingCondition>>(
                        (l, r) => l == null && r == null ||
                                 (l != null && r != null &&
                                  JsonSerializer.Serialize(l, jsonOptions) == JsonSerializer.Serialize(r, jsonOptions)),
                        v => v == null ? 0 : JsonSerializer.Serialize(v, jsonOptions).GetHashCode(),
                        v => v == null ? null :
                             JsonSerializer.Deserialize<List<PirepIcingCondition>>(
                                 JsonSerializer.Serialize(v ?? new List<PirepIcingCondition>(), jsonOptions),
                                 jsonOptions)
                    ));
        }
    }
}
