using VFR3D.Domain.Enums;

namespace VFR3D.Infrastructure.Dtos.Aircraft;

public record UpdateAircraftRequestDto
{
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
    public AirspeedUnits AirspeedUnits { get; init; } = AirspeedUnits.Knots;
    public LengthUnits LengthUnits { get; init; } = LengthUnits.Feet;
    public int? DefaultCruiseAltitude { get; init; }
    public int? MaxCeiling { get; init; }
    public int? GlideSpeed { get; init; }
    public double? GlideRatio { get; init; }
}
