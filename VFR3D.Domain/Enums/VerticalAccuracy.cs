using System.Text.Json.Serialization;

namespace VFR3D.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VerticalAccuracy
{
    Unknown,
    Within3Feet,
    Within10Feet,
    Within20Feet,
    Within50Feet,
    Within125Feet,
    Within250Feet,
    Within500Feet,
    Within1000Feet
}
