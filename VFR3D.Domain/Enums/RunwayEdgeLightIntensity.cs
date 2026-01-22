using System.Text.Json.Serialization;

namespace VFR3D.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RunwayEdgeLightIntensity
{
    Unknown,
    None,
    High,
    Medium,
    Low,
    Flood,
    NonStandard,
    Perimeter,
    Strobe
}
