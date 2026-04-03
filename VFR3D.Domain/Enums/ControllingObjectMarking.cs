using System.Text.Json.Serialization;

namespace VFR3D.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ControllingObjectMarking
{
    Unknown,
    None,
    Marked,
    Lighted,
    MarkedAndLighted
}
