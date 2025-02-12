using VFR3D.Domain.ValueObjects.Taf;

namespace VFR3D.Infrastructure.Dtos;

public class TafDto
{
    public string? RawText { get; init; }
    public string? StationId { get; init; }
    public string? IssueTime { get; init; }
    public string? BulletinTime { get; init; }
    public string? ValidTimeFrom { get; init; }
    public string? ValidTimeTo { get; init; }
    public string? Remarks { get; init; }
    public float? Latitude { get; init; }
    public float? Longitude { get; init; }
    public float? ElevationM { get; init; }
    public List<TafForecast>? Forecast { get; init; }
}