using VFR3D.Domain.Entities;

namespace VFR3D.Infrastructure.Dtos.Mappers;

public static class AirsigmetMapper
{
    public static AirsigmetDto ToDto(Airsigmet airsigmet)
    {
        return new AirsigmetDto
        {
            Id = airsigmet.Id,
            RawText = airsigmet.RawText,
            ValidTimeFrom = airsigmet.ValidTimeFrom,
            ValidTimeTo = airsigmet.ValidTimeTo,
            Altitude = airsigmet.Altitude,
            MovementDirDegrees = airsigmet.MovementDirDegrees,
            MovementSpeedKt = airsigmet.MovementSpeedKt,
            Hazard = airsigmet.Hazard,
            AirsigmetType = airsigmet.AirsigmetType,
            Areas = airsigmet.Areas
        };
    }
}