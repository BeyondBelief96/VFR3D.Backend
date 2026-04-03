using VFR3D.Domain.Entities;
using VFR3D.Domain.Enums;
using VFR3D.Domain.ValueObjects.Airsigmets;

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
            Hazard = MapHazard(airsigmet.Hazard),
            AirsigmetType = airsigmet.AirsigmetType,
            Areas = airsigmet.Areas
        };
    }

    private static AirsigmetHazardDto? MapHazard(AirsigmetHazard? hazard)
    {
        if (hazard == null) return null;

        return new AirsigmetHazardDto
        {
            Type = ParseHazardType(hazard.Type),
            Severity = hazard.Severity
        };
    }

    private static AirsigmetHazardType? ParseHazardType(string? type)
    {
        if (string.IsNullOrEmpty(type)) return null;

        return type.ToUpperInvariant() switch
        {
            "CONVECTIVE" => AirsigmetHazardType.CONVECTIVE,
            "ICE" => AirsigmetHazardType.ICE,
            "TURB" => AirsigmetHazardType.TURB,
            "IFR" => AirsigmetHazardType.IFR,
            "MTN OBSCN" => AirsigmetHazardType.MTN_OBSCN,
            _ => null
        };
    }
}