namespace VFR3D.Domain.ValueObjects.Taf
{
    public class TafSkyCondition
    {
        public string SkyCover { get; set; } = string.Empty;
        public int? CloudBaseFtAgl { get; set; }
        public string? CloudType { get; set; }
    }
}
