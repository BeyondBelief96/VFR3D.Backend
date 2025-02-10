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

            builder.HasIndex(e => e.ObservationTime);
            builder.HasIndex(e => e.ReceiptTime);
            builder.HasIndex(e => new { e.Latitude, e.Longitude });

            builder.Property(x => x.QualityControlFlags)
                .HasConversion(
                    v => v == null ? "{}" : JsonSerializer.Serialize(v, jsonOptions),
                    v => string.IsNullOrEmpty(v) ? new PirepQualityControlFlags() : JsonSerializer.Deserialize<PirepQualityControlFlags>(v, jsonOptions) ?? new PirepQualityControlFlags(),
                    new ValueComparer<PirepQualityControlFlags>(
                        (l, r) => (l == null && r == null) || (l != null && r != null && JsonSerializer.Serialize(l, jsonOptions) == JsonSerializer.Serialize(r, jsonOptions)),
                        v => v == null ? 0 : JsonSerializer.Serialize(v, jsonOptions).GetHashCode(),
                        v => v == null ? new PirepQualityControlFlags() : JsonSerializer.Deserialize<PirepQualityControlFlags>(JsonSerializer.Serialize(v, jsonOptions), jsonOptions) ?? new PirepQualityControlFlags()
                    ));

            builder.Property(x => x.SkyConditions)
                .HasConversion(
                    v => v == null ? "[]" : JsonSerializer.Serialize(v, jsonOptions),
                    v => string.IsNullOrEmpty(v) ? new List<PirepSkyCondition>() : JsonSerializer.Deserialize<List<PirepSkyCondition>>(v, jsonOptions) ?? new List<PirepSkyCondition>(),
                    new ValueComparer<List<PirepSkyCondition>>(
                        (l, r) => (l == null && r == null) || (l != null && r != null && JsonSerializer.Serialize(l, jsonOptions) == JsonSerializer.Serialize(r, jsonOptions)),
                        v => v == null ? 0 : JsonSerializer.Serialize(v, jsonOptions).GetHashCode(),
                        v => v == null ? new List<PirepSkyCondition>() : JsonSerializer.Deserialize<List<PirepSkyCondition>>(JsonSerializer.Serialize(v, jsonOptions), jsonOptions) ?? new List<PirepSkyCondition>()
                    ));

            builder.Property(x => x.TurbulenceConditions)
                .HasConversion(
                    v => v == null ? "[]" : JsonSerializer.Serialize(v, jsonOptions),
                    v => string.IsNullOrEmpty(v) ? new List<PirepTurbulenceCondition>() : JsonSerializer.Deserialize<List<PirepTurbulenceCondition>>(v, jsonOptions) ?? new List<PirepTurbulenceCondition>(),
                    new ValueComparer<List<PirepTurbulenceCondition>>(
                        (l, r) => (l == null && r == null) || (l != null && r != null && JsonSerializer.Serialize(l, jsonOptions) == JsonSerializer.Serialize(r, jsonOptions)),
                        v => v == null ? 0 : JsonSerializer.Serialize(v, jsonOptions).GetHashCode(),
                        v => v == null ? new List<PirepTurbulenceCondition>() : JsonSerializer.Deserialize<List<PirepTurbulenceCondition>>(JsonSerializer.Serialize(v, jsonOptions), jsonOptions) ?? new List<PirepTurbulenceCondition>()
                    ));

            builder.Property(x => x.IcingConditions)
                .HasConversion(
                    v => v == null ? "[]" : JsonSerializer.Serialize(v, jsonOptions),
                    v => string.IsNullOrEmpty(v) ? new List<PirepIcingCondition>() : JsonSerializer.Deserialize<List<PirepIcingCondition>>(v, jsonOptions) ?? new List<PirepIcingCondition>(),
                    new ValueComparer<List<PirepIcingCondition>>(
                        (l, r) => (l == null && r == null) || (l != null && r != null && JsonSerializer.Serialize(l, jsonOptions) == JsonSerializer.Serialize(r, jsonOptions)),
                        v => v == null ? 0 : JsonSerializer.Serialize(v, jsonOptions).GetHashCode(),
                        v => v == null ? new List<PirepIcingCondition>() : JsonSerializer.Deserialize<List<PirepIcingCondition>>(JsonSerializer.Serialize(v, jsonOptions), jsonOptions) ?? new List<PirepIcingCondition>()
                    ));
        }
    }
}