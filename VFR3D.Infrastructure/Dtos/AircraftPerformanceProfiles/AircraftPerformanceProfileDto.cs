using VFR3D.Domain.Enums;

namespace VFR3D.Infrastructure.Dtos.AircraftPerformanceProfiles;

public record AircraftPerformanceProfileDto
{
    public string Id { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public string? AircraftId { get; init; }
    public string ProfileName { get; init; } = string.Empty;
    public int ClimbTrueAirspeed { get; init; }
    public int CruiseTrueAirspeed { get; init; }
    public double CruiseFuelBurn { get; init; }
    public double ClimbFuelBurn { get; init; }
    public double DescentFuelBurn { get; init; }
    public int ClimbFpm { get; init; }
    public int DescentFpm { get; init; }
    public int DescentTrueAirspeed { get; init; }
    public double SttFuelGals { get; init; }
    public double FuelOnBoardGals { get; init; }

    /// <summary>
    /// Indicates the airspeed units used for the values in this DTO.
    /// </summary>
    public AirspeedUnits AirspeedUnits { get; init; } = AirspeedUnits.Knots;

    /// <summary>
    /// Indicates the length units used for the values in this DTO.
    /// </summary>
    public LengthUnits LengthUnits { get; init; } = LengthUnits.Feet;
}