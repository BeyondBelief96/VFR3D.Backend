using VFR3D.Domain.Entities;

namespace VFR3D.Infrastructure.Dtos.Mappers;

public static class TafMapper
{
    public static TafDto ToDto(Taf taf)
    {
        return new TafDto
        {
            RawText = taf.RawText,
            StationId = taf.StationId,
            IssueTime = taf.IssueTime,
            BulletinTime = taf.BulletinTime,
            ValidTimeFrom = taf.ValidTimeFrom,
            ValidTimeTo = taf.ValidTimeTo,
            Remarks = taf.Remarks,
            Latitude = taf.Latitude,
            Longitude = taf.Longitude,
            ElevationM = taf.ElevationM,
            Forecast = taf.Forecast
        };
    }
}