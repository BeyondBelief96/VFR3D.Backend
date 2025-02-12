namespace VFR3D.Infrastructure.Dtos
{
    public record MetarDto
    {
        public string? RawText { get; init; }
        public string? StationId { get; init; }
        public string? ObservationTime { get; init; }
        public float? Latitude { get; init; }
        public float? Longitude { get; init; }
        public float? TempC { get; init; }
        public float? DewpointC { get; init; }
        public string? WindDirDegrees { get; init; }
        public int? WindSpeedKt { get; init; }
        public int? WindGustKt { get; init; }
        public string? VisibilityStatuteMi { get; init; }
        public float? AltimInHg { get; init; }
        public float? SeaLevelPressureMb { get; init; }
        public MetarQualityControlFlagsDto? QualityControlFlags { get; init; }
        public string? WxString { get; init; }
        public List<MetarSkyConditionDto>? SkyCondition { get; init; } = [];
        public string? FlightCategory { get; init; }
    }

    public record MetarQualityControlFlagsDto
    {
        public string? Corrected { get; init; }
        public string? Auto { get; init; }
        public string? AutoStation { get; init; }
        public string? MaintenanceIndicatorOn { get; init; }
        public string? NoSignal { get; init; }
        public string? LightningSensorOff { get; init; }
        public string? FreezingRainSensorOff { get; init; }
        public string? PresentWeatherSensorOff { get; init; }
    }

    public record MetarSkyConditionDto
    {
        public string SkyCover { get; init; } = string.Empty;
        public int? CloudBaseFtAgl { get; init; }
    }
}
