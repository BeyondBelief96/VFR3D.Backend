using System.Text.Json.Serialization;

namespace VFR3D.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum InstrumentApproachType
{
    Unknown,
    None,
    Ils,
    Mls,
    Sdf,
    Localizer,
    Lda,
    Ismls,
    IlsDme,
    SdfDme,
    LocDme,
    LocGs,
    LdaDme
}
