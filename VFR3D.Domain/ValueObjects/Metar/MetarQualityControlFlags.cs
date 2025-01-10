namespace VFR3D.Domain.ValueObjects.Metar
{
    public class MetarQualityControlFlags
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
}
