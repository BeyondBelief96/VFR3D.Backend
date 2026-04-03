using System.Text.Json.Serialization;

namespace VFR3D.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RunwayMarkingsType
{
    Unknown,
    None,
    PrecisionInstrument,
    NonPrecisionInstrument,
    Basic,
    NumbersOnly,
    NonStandard,
    Buoys,
    Stol
}
