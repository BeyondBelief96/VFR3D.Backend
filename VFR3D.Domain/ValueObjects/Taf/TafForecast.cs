namespace VFR3D.Domain.ValueObjects.Taf
{
    public class TafForecast
    {
        public string? FcstTimeFrom { get; set; }
        public string? FcstTimeTo { get; set; }
        public string? ChangeIndicator { get; set; }
        public string? TimeBecoming { get; set; }
        public int? Probability { get; set; }
        public string? WindDirDegrees { get; set; } // String to handle "VRB" values
        public int? WindSpeedKt { get; set; }
        public int? WindGustKt { get; set; }
        public short? WindShearHgtFtAgl { get; set; }  // Changed to short per schema
        public short? WindShearDirDegrees { get; set; }  // Changed to short per schema
        public int? WindShearSpeedKt { get; set; }
        public string? VisibilityStatuteMi { get; set; }  // String to handle "6+" values
        public float? AltimInHg { get; set; }
        public short? VertVisFt { get; set; }  // Changed to short per schema
        public string? WxString { get; set; }
        public string? NotDecoded { get; set; }
        public List<TafSkyCondition>? SkyConditions { get; set; }
        public List<TafTurbulenceCondition>? TurbulenceConditions { get; set; }
        public List<TafIcingCondition>? IcingConditions { get; set; }
        public List<TafTemperature>? Temperature { get; set; }
    }
}
