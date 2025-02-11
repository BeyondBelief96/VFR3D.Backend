namespace VFR3D.Infrastructure.Dtos
{
    public class MetarDto
    {
        public string? RawText { get; set; }
        public string? StationId { get; set; }
        public string? ObservationTime { get; set; }
        public float? Latitude { get; set; }
        public float? Longitude { get; set; }
        public float? TempC { get; set; }
        public float? DewpointC { get; set; }
        public string? WindDirDegrees { get; set; }
        public int? WindSpeedKt { get; set; }
        public int? WindGustKt { get; set; }
        public string? VisibilityStatuteMi { get; set; }
        public float? AltimInHg { get; set; }
        public float? SeaLevelPressureMb { get; set; }
        public MetarQualityControlFlagsDto? QualityControlFlags { get; set; }
        public string? WxString { get; set; }
        public List<MetarSkyConditionDto>? SkyCondition { get; set; }
        public string? FlightCategory { get; set; }
    }

    public class MetarQualityControlFlagsDto
    {
        public string? Corrected { get; set; }
        public string? Auto { get; set; }
        public string? AutoStation { get; set; }
        public string? MaintenanceIndicatorOn { get; set; }
        public string? NoSignal { get; set; }
        public string? LightningSensorOff { get; set; }
        public string? FreezingRainSensorOff { get; set; }
        public string? PresentWeatherSensorOff { get; set; }
    }

    public class MetarSkyConditionDto
    {
        public string SkyCover { get; set; } = string.Empty;
        public int? CloudBaseFtAgl { get; set; }
    }
}
