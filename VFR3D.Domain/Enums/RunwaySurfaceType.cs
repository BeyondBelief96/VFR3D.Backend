using System.Text.Json.Serialization;

namespace VFR3D.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RunwaySurfaceType
{
    Unknown,
    Concrete,
    Asphalt,
    Snow,
    Ice,
    Mats,
    Treated,
    Gravel,
    Turf,
    Dirt,
    PartiallyPaved,
    Rooftop,
    Water,
    Aluminum,
    Brick,
    Caliche,
    Coral,
    Deck,
    Grass,
    Metal,
    NonStandard,
    OilChip,
    Psp,
    Sand,
    Sod,
    Steel,
    Wood
}
