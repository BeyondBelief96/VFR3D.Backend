using VFR3D.Domain.Entities;

namespace VFR3D.Infrastructure.Dtos.Mappers;

public static class PirepMapper
{
    public static PirepDto ToDto(Pirep pirep)
    {
        return new PirepDto
        {
            Id = pirep.Id,
            RawText = pirep.RawText,
            ReceiptTime = pirep.ReceiptTime,
            ObservationTime = pirep.ObservationTime,
            QualityControlFlags = pirep.QualityControlFlags,
            AircraftRef = pirep.AircraftRef,
            Latitude = pirep.Latitude,
            Longitude = pirep.Longitude,
            AltitudeFtMsl = pirep.AltitudeFtMsl,
            SkyConditions = pirep.SkyConditions,
            TurbulenceConditions = pirep.TurbulenceConditions,
            IcingConditions = pirep.IcingConditions,
            VisibilityStatuteMi = pirep.VisibilityStatuteMi,
            WxString = pirep.WxString,
            TempC = pirep.TempC,
            WindDirDegrees = pirep.WindDirDegrees,
            WindSpeedKt = pirep.WindSpeedKt,
            VertGustKt = pirep.VertGustKt,
            ReportType = pirep.ReportType
        };
    }
}