namespace VFR3D.Infrastructure.Dtos.Navlog;

public record WindsAloftSiteDto
{
    public string Id { get; init; } = string.Empty;
    public float Lat { get; init; }
    public float Lon { get; init; }
    public Dictionary<string, WindTempDto> WindTemp { get; init; } = new();
}