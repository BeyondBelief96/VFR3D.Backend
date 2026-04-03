using System.Text.Json.Serialization;

namespace VFR3D.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RunwayMarkingsCondition
{
    Unknown,
    Good,
    Fair,
    Poor
}
