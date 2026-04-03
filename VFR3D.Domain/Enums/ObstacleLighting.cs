using System.Text.Json.Serialization;

namespace VFR3D.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ObstacleLighting
{
    Unknown,
    Red,
    DualMediumWhiteStrobeRed,
    HighIntensityWhiteStrobeRed,
    MediumIntensityWhiteStrobe,
    HighIntensityWhiteStrobe,
    Flood,
    DualMediumCatenary,
    SynchronizedRedLighting,
    Lighted,
    None
}
