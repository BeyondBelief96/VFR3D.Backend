using System.Text.Json.Serialization;

namespace VFR3D.Infrastructure.Dtos.Notam;

public record NmsTokenResponseDto
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; } = string.Empty;

    [JsonPropertyName("issued_at")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long IssuedAt { get; init; }

    [JsonPropertyName("expires_in")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int ExpiresIn { get; init; }
}
