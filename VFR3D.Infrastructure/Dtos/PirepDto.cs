using VFR3D.Domain.ValueObjects.Pireps;

namespace VFR3D.Infrastructure.Dtos
{
    public class PirepDto
    {
        public int Id { get; init; }
        public string? RawText { get; init; }
        public string? ReceiptTime { get; init; }
        public string? ObservationTime { get; init; }
        public PirepQualityControlFlags? QualityControlFlags { get; init; }
        public string? AircraftRef { get; init; }
        public float? Latitude { get; init; }
        public float? Longitude { get; init; }
        public int? AltitudeFtMsl { get; init; }
        public List<PirepSkyCondition>? SkyConditions { get; init; } = [];
        public List<PirepTurbulenceCondition>? TurbulenceConditions { get; init; } = [];
        public List<PirepIcingCondition>? IcingConditions { get; init; } = [];
        public int? VisibilityStatuteMi { get; init; }
        public string? WxString { get; init; }
        public float? TempC { get; init; }
        public int? WindDirDegrees { get; init; }
        public int? WindSpeedKt { get; init; }
        public int? VertGustKt { get; init; }
        public string? ReportType { get; init; }
    }
}
