namespace VFR3D.Infrastructure.Dtos.Navlog;

public record WindTempDto
{
    public int? Direction { get; init; }
    public int Speed { get; init; }
    public float? Temperature { get; init; }
}