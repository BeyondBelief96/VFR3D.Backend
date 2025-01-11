namespace VFR3D.Domain.ValueObjects.Pireps
{
    public class PirepTurbulenceCondition
    {
        public string? TurbulenceType { get; set; }      // CAT, CHOP, LLWS, MWAVE
        public string? TurbulenceIntensity { get; set; } // NEG, SMTH-LGT, LGT, etc.
        public int? TurbulenceBaseFtMsl { get; set; }
        public int? TurbulenceTopFtMsl { get; set; }
        public string? TurbulenceFreq { get; set; }      // ISOL, OCNL, CONT
    }
}
