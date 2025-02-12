using VFR3D.Domain.ValueObjects.Taf;

namespace VFR3D.Infrastructure.Dtos;

public class TafDto
{
    public string? RawText { get; set; }
    public string? StationId { get; set; }
    public string? IssueTime { get; set; }
    public string? BulletinTime { get; set; }
    public string? ValidTimeFrom { get; set; }
    public string? ValidTimeTo { get; set; }
    public string? Remarks { get; set; }
    public float? Latitude { get; set; }
    public float? Longitude { get; set; }
    public float? ElevationM { get; set; }
    public List<TafForecast>? Forecast { get; set; }
}