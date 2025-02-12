using VFR3D.Domain.ValueObjects.Pireps;

namespace VFR3D.Infrastructure.Dtos
{
    public class PirepDto
    {
        public int Id { get; set; }
        public string? RawText { get; set; }
        public string? ReceiptTime { get; set; }
        public string? ObservationTime { get; set; }
        public PirepQualityControlFlags? QualityControlFlags { get; set; }
        public string? AircraftRef { get; set; }
        public float? Latitude { get; set; }
        public float? Longitude { get; set; }
        public int? AltitudeFtMsl { get; set; }
        public List<PirepSkyCondition>? SkyConditions { get; set; }
        public List<PirepTurbulenceCondition>? TurbulenceConditions { get; set; }
        public List<PirepIcingCondition>? IcingConditions { get; set; }
        public int? VisibilityStatuteMi { get; set; }
        public string? WxString { get; set; }
        public float? TempC { get; set; }
        public int? WindDirDegrees { get; set; }
        public int? WindSpeedKt { get; set; }
        public int? VertGustKt { get; set; }
        public string? ReportType { get; set; }
    }
}
