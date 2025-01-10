namespace VFR3D.Domain.ValueObjects.Metar
{
    public class MetarSkyCondition
    {
        public string SkyCover { get; set; } = string.Empty;
        public int? CloudBaseFtAgl { get; set; }
    }
}
