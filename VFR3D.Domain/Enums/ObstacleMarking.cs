using System.Text.Json.Serialization;

namespace VFR3D.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ObstacleMarking
{
    Unknown,
    OrangeOrOrangeWhitePaint,
    WhitePaintOnly,
    Marked,
    FlagMarker,
    SphericalMarker,
    None
}
