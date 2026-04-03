using System.Text.Json.Serialization;

namespace VFR3D.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AircraftCategory
{
    SingleEngine,
    MultiEngine,
    Helicopter,
    Glider,
    Balloon,
    Ultralight,
    LightSport,
    Gyroplane
}
