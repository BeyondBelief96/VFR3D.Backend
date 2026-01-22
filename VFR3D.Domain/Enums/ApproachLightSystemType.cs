using System.Text.Json.Serialization;

namespace VFR3D.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ApproachLightSystemType
{
    Unknown,
    None,
    AirForceOverrun,
    Alsaf,
    Alsf1,
    Alsf2,
    Mals,
    Malsf,
    Malsr,
    Rail,
    Sals,
    Salsf,
    Ssals,
    Ssalf,
    Ssalr,
    Odals,
    Rlls,
    MilitaryOverrun,
    NonStandard
}
