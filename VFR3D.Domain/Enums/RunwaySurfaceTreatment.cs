using System.Text.Json.Serialization;

namespace VFR3D.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RunwaySurfaceTreatment
{
    Unknown,
    None,
    Grooved,
    PorousFrictionCourse,
    AggregateFrictionSealCoat,
    RubberizedFrictionSealCoat,
    WireComb
}
