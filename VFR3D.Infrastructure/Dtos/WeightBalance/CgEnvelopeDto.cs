namespace VFR3D.Infrastructure.Dtos.WeightBalance;

public record CgEnvelopeDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public List<CgEnvelopePointDto> Limits { get; init; } = [];
}
