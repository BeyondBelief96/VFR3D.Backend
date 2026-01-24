using VFR3D.Domain.Enums;

namespace VFR3D.Infrastructure.Dtos;

public record ObstacleDto
{
    public string OasNumber { get; init; } = string.Empty;
    public string? StateId { get; init; }
    public string? CityName { get; init; }
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
    public string? ObstacleType { get; init; }
    public int? Quantity { get; init; }
    public int? HeightAgl { get; init; }
    public int? HeightAmsl { get; init; }
    public ObstacleLighting Lighting { get; init; }
    public HorizontalAccuracy HorizontalAccuracy { get; init; }
    public VerticalAccuracy VerticalAccuracy { get; init; }
    public ObstacleMarking Marking { get; init; }
    public VerificationStatus VerificationStatus { get; init; }
}
