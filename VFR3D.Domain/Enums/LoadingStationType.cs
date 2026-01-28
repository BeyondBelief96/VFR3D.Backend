using System.Text.Json.Serialization;

namespace VFR3D.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LoadingStationType
{
    Standard,   // Weight entered directly in pounds/kg
    Fuel,       // Capacity in gallons, weight per gallon
    Oil         // Capacity in quarts, weight per quart
}
