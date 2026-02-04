namespace VFR3D.Infrastructure.Dtos;

public record AirportDiagramDto
{
    public string ChartName { get; init; } = string.Empty;
    public string PdfUrl { get; init; } = string.Empty;
}

public record AirportDiagramsResponseDto
{
    public string AirportName { get; init; } = string.Empty;
    public string? IcaoIdent { get; init; }
    public string? AirportIdent { get; init; }
    public List<AirportDiagramDto> Diagrams { get; init; } = new();
}