using VFR3D.Domain.Enums;
using VFR3D.Infrastructure.Dtos.AircraftPerformanceProfiles;

namespace VFR3D.Infrastructure.Dtos.Aircraft;

public record AircraftDto
{
    public string Id { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public string TailNumber { get; init; } = string.Empty;
    public string AircraftType { get; init; } = string.Empty;
    public string? CallSign { get; init; }
    public string? SerialNumber { get; init; }
    public string? PrimaryColor { get; init; }
    public string? Color2 { get; init; }
    public string? Color3 { get; init; }
    public string? Color4 { get; init; }
    public AircraftCategory Category { get; init; }
    public string? AircraftHome { get; init; }
    public AirspeedUnits AirspeedUnits { get; init; }
    public LengthUnits LengthUnits { get; init; }
    public int? DefaultCruiseAltitude { get; init; }
    public int? MaxCeiling { get; init; }
    public int? GlideSpeed { get; init; }
    public double? GlideRatio { get; init; }
    public List<AircraftPerformanceProfileDto> PerformanceProfiles { get; init; } = [];
}
