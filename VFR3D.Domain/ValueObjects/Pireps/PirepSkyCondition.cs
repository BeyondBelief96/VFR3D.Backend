namespace VFR3D.Domain.ValueObjects.Pireps
{
    public class PirepSkyCondition
    {
        public string SkyCover { get; set; } = string.Empty;
        public int? CloudBaseFtMsl { get; set; }
        public int? CloudTopFtMsl { get; set; }
    }
}
