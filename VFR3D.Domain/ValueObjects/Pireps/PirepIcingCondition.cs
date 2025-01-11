namespace VFR3D.Domain.ValueObjects.Pireps
{
    public class PirepIcingCondition
    {
        public string? IcingType { get; set; }      // RIME, CLEAR, MIXED
        public string? IcingIntensity { get; set; } // NEG, NEGclr, TRC, etc.
        public int? IcingBaseFtMsl { get; set; }
        public int? IcingTopFtMsl { get; set; }
    }
}
