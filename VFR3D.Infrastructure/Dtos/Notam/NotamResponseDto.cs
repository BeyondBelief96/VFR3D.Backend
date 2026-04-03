namespace VFR3D.Infrastructure.Dtos.Notam;

/// <summary>
/// Response wrapper for NOTAM queries
/// </summary>
public record NotamResponseDto
{
    public List<NotamDto> Notams { get; init; } = [];
    public int TotalCount { get; init; }
    public DateTime RetrievedAt { get; init; } = DateTime.UtcNow;
    public string? QueryLocation { get; init; }
}
