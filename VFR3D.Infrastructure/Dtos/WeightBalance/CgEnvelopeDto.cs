using VFR3D.Domain.Enums;

namespace VFR3D.Infrastructure.Dtos.WeightBalance;

public record CgEnvelopeDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public CgEnvelopeFormat Format { get; init; } = CgEnvelopeFormat.Arm;
    public List<CgEnvelopePointDto> Limits { get; init; } = [];
}
