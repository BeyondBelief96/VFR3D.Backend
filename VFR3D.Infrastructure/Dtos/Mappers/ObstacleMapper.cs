using VFR3D.Domain.Entities;
using VFR3D.Domain.Enums;

namespace VFR3D.Infrastructure.Dtos.Mappers;

public static class ObstacleMapper
{
    public static ObstacleDto ToDto(Obstacle obstacle)
    {
        return new ObstacleDto
        {
            OasNumber = obstacle.OasNumber,
            StateId = obstacle.StateId,
            CityName = obstacle.CityName?.Trim(),
            Latitude = obstacle.LatDecimal,
            Longitude = obstacle.LongDecimal,
            ObstacleType = obstacle.ObstacleType?.Trim(),
            Quantity = obstacle.Quantity,
            HeightAgl = obstacle.HeightAgl,
            HeightAmsl = obstacle.HeightAmsl,
            Lighting = ParseLighting(obstacle.Lighting),
            HorizontalAccuracy = ParseHorizontalAccuracy(obstacle.HorizontalAccuracy),
            VerticalAccuracy = ParseVerticalAccuracy(obstacle.VerticalAccuracy),
            Marking = ParseMarking(obstacle.MarkIndicator),
            VerificationStatus = ParseVerificationStatus(obstacle.VerificationStatus)
        };
    }

    private static ObstacleLighting ParseLighting(string? code)
    {
        return code?.ToUpperInvariant() switch
        {
            "R" => ObstacleLighting.Red,
            "D" => ObstacleLighting.DualMediumWhiteStrobeRed,
            "H" => ObstacleLighting.HighIntensityWhiteStrobeRed,
            "M" => ObstacleLighting.MediumIntensityWhiteStrobe,
            "S" => ObstacleLighting.HighIntensityWhiteStrobe,
            "F" => ObstacleLighting.Flood,
            "C" => ObstacleLighting.DualMediumCatenary,
            "W" => ObstacleLighting.SynchronizedRedLighting,
            "L" => ObstacleLighting.Lighted,
            "N" => ObstacleLighting.None,
            _ => ObstacleLighting.Unknown
        };
    }

    private static HorizontalAccuracy ParseHorizontalAccuracy(string? code)
    {
        return code?.ToUpperInvariant() switch
        {
            "1" => HorizontalAccuracy.Within20Feet,
            "2" => HorizontalAccuracy.Within50Feet,
            "3" => HorizontalAccuracy.Within100Feet,
            "4" => HorizontalAccuracy.Within250Feet,
            "5" => HorizontalAccuracy.Within500Feet,
            "6" => HorizontalAccuracy.Within1000Feet,
            "7" => HorizontalAccuracy.WithinHalfNauticalMile,
            "8" => HorizontalAccuracy.Within1NauticalMile,
            _ => HorizontalAccuracy.Unknown
        };
    }

    private static VerticalAccuracy ParseVerticalAccuracy(string? code)
    {
        return code?.ToUpperInvariant() switch
        {
            "A" => VerticalAccuracy.Within3Feet,
            "B" => VerticalAccuracy.Within10Feet,
            "C" => VerticalAccuracy.Within20Feet,
            "D" => VerticalAccuracy.Within50Feet,
            "E" => VerticalAccuracy.Within125Feet,
            "F" => VerticalAccuracy.Within250Feet,
            "G" => VerticalAccuracy.Within500Feet,
            "H" => VerticalAccuracy.Within1000Feet,
            _ => VerticalAccuracy.Unknown
        };
    }

    private static ObstacleMarking ParseMarking(string? code)
    {
        return code?.ToUpperInvariant() switch
        {
            "P" => ObstacleMarking.OrangeOrOrangeWhitePaint,
            "W" => ObstacleMarking.WhitePaintOnly,
            "M" => ObstacleMarking.Marked,
            "F" => ObstacleMarking.FlagMarker,
            "S" => ObstacleMarking.SphericalMarker,
            "N" => ObstacleMarking.None,
            _ => ObstacleMarking.Unknown
        };
    }

    private static VerificationStatus ParseVerificationStatus(string? code)
    {
        return code?.ToUpperInvariant() switch
        {
            "O" => VerificationStatus.Verified,
            "U" => VerificationStatus.Unverified,
            _ => VerificationStatus.Unknown
        };
    }
}
