namespace VFR3D.Infrastructure.Dtos.Navlog;

public record WindsAloftDto
{
    public DateTime ValidTime { get; init; }
    public DateTime ForUseStartTime { get; init; }
    public DateTime ForUseEndTime { get; init; }
    public List<WindsAloftSiteDto> WindTemp  { get; init; } = [];
}