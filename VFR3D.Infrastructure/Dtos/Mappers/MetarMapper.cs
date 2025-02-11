using Vfr3d.Domain.Entities;
using VFR3D.Infrastructure.Dtos;

namespace VFR3D.Infrastructure.Dtos.Mappers;

public static class MetarMapper
{
    public static MetarDto ToDto(Metar metar)
    {
        return new MetarDto
        {
            RawText = metar.RawText,
            StationId = metar.StationId,
            ObservationTime = metar.ObservationTime,
            Latitude = metar.Latitude,
            Longitude = metar.Longitude,
            TempC = metar.TempC,
            DewpointC = metar.DewpointC,
            WindDirDegrees = metar.WindDirDegrees,
            WindSpeedKt = metar.WindSpeedKt,
            WindGustKt = metar.WindGustKt,
            VisibilityStatuteMi = metar.VisibilityStatuteMi,
            AltimInHg = metar.AltimInHg,
            SeaLevelPressureMb = metar.SeaLevelPressureMb,
            QualityControlFlags = metar.QualityControlFlags != null
                ? new MetarQualityControlFlagsDto
                {
                    Corrected = metar.QualityControlFlags.Corrected,
                    Auto = metar.QualityControlFlags.Auto,
                    AutoStation = metar.QualityControlFlags.AutoStation,
                    MaintenanceIndicatorOn = metar.QualityControlFlags.MaintenanceIndicatorOn,
                    NoSignal = metar.QualityControlFlags.NoSignal,
                    LightningSensorOff = metar.QualityControlFlags.LightningSensorOff,
                    FreezingRainSensorOff = metar.QualityControlFlags.FreezingRainSensorOff,
                    PresentWeatherSensorOff = metar.QualityControlFlags.PresentWeatherSensorOff
                }
                : null,
            WxString = metar.WxString,
            SkyCondition = metar.SkyCondition?.Select(sc => new MetarSkyConditionDto
            {
                SkyCover = sc.SkyCover,
                CloudBaseFtAgl = sc.CloudBaseFtAgl
            }).ToList(),
            FlightCategory = metar.FlightCategory
        };
    }
}