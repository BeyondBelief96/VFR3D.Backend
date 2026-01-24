using System.Text.Json.Serialization;

namespace VFR3D.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VerificationStatus
{
    Unknown,
    Verified,
    Unverified
}
