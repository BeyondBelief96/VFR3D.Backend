using VFR3D.Domain.Entities;
using VFR3D.Domain.Enums;
using VFR3D.Domain.Utilities.UnitConversions;
using VFR3D.Infrastructure.Dtos.AircraftPerformanceProfiles;

namespace VFR3D.Infrastructure.Dtos.Mappers;

public static class AircraftPerformanceProfileMapper
{
    /// <summary>
    /// Maps an entity to a DTO without unit conversion (assumes canonical units).
    /// </summary>
    public static AircraftPerformanceProfileDto MapToDto(AircraftPerformanceProfile entity)
    {
        return new AircraftPerformanceProfileDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            AircraftId = entity.AircraftId,
            ProfileName = entity.ProfileName,
            ClimbTrueAirspeed = entity.ClimbTrueAirspeed,
            CruiseTrueAirspeed = entity.CruiseTrueAirspeed,
            CruiseFuelBurn = entity.CruiseFuelBurn,
            ClimbFuelBurn = entity.ClimbFuelBurn,
            DescentFuelBurn = entity.DescentFuelBurn,
            ClimbFpm = entity.ClimbFpm,
            DescentFpm = entity.DescentFpm,
            DescentTrueAirspeed = entity.DescentTrueAirspeed,
            SttFuelGals = entity.SttFuelGals,
            FuelOnBoardGals = entity.FuelOnBoardGals,
            AirspeedUnits = AirspeedUnits.Knots,
            LengthUnits = LengthUnits.Feet
        };
    }

    /// <summary>
    /// Maps an entity to a DTO with unit conversion from canonical units (knots/feet) to user units.
    /// </summary>
    public static AircraftPerformanceProfileDto MapToDto(
        AircraftPerformanceProfile entity,
        AirspeedUnits airspeedUnits,
        LengthUnits lengthUnits)
    {
        return new AircraftPerformanceProfileDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            AircraftId = entity.AircraftId,
            ProfileName = entity.ProfileName,
            ClimbTrueAirspeed = AirspeedConversion.FromKnotsInt(entity.ClimbTrueAirspeed, airspeedUnits),
            CruiseTrueAirspeed = AirspeedConversion.FromKnotsInt(entity.CruiseTrueAirspeed, airspeedUnits),
            CruiseFuelBurn = entity.CruiseFuelBurn,
            ClimbFuelBurn = entity.ClimbFuelBurn,
            DescentFuelBurn = entity.DescentFuelBurn,
            ClimbFpm = LengthConversion.FromFeetInt(entity.ClimbFpm, lengthUnits),
            DescentFpm = LengthConversion.FromFeetInt(entity.DescentFpm, lengthUnits),
            DescentTrueAirspeed = AirspeedConversion.FromKnotsInt(entity.DescentTrueAirspeed, airspeedUnits),
            SttFuelGals = entity.SttFuelGals,
            FuelOnBoardGals = entity.FuelOnBoardGals,
            AirspeedUnits = airspeedUnits,
            LengthUnits = lengthUnits
        };
    }

    public static AircraftPerformanceProfile MapToEntity(AircraftPerformanceProfileDto dto)
    {
        return new AircraftPerformanceProfile
        {
            Id = dto.Id,
            UserId = dto.UserId,
            AircraftId = dto.AircraftId,
            ProfileName = dto.ProfileName,
            ClimbTrueAirspeed = dto.ClimbTrueAirspeed,
            CruiseTrueAirspeed = dto.CruiseTrueAirspeed,
            CruiseFuelBurn = dto.CruiseFuelBurn,
            ClimbFuelBurn = dto.ClimbFuelBurn,
            DescentFuelBurn = dto.DescentFuelBurn,
            ClimbFpm = dto.ClimbFpm,
            DescentFpm = dto.DescentFpm,
            DescentTrueAirspeed = dto.DescentTrueAirspeed,
            SttFuelGals = dto.SttFuelGals,
            FuelOnBoardGals = dto.FuelOnBoardGals
        };
    }

    /// <summary>
    /// Creates a new entity from request without unit conversion (assumes canonical units).
    /// </summary>
    public static AircraftPerformanceProfile CreateFromRequest(string userId, SaveAircraftPerformanceProfileRequestDto request)
    {
        return new AircraftPerformanceProfile
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            AircraftId = request.AircraftId,
            ProfileName = request.ProfileName,
            ClimbTrueAirspeed = request.ClimbTrueAirspeed,
            CruiseTrueAirspeed = request.CruiseTrueAirspeed,
            CruiseFuelBurn = request.CruiseFuelBurn,
            ClimbFuelBurn = request.ClimbFuelBurn,
            DescentFuelBurn = request.DescentFuelBurn,
            ClimbFpm = request.ClimbFpm,
            DescentFpm = request.DescentFpm,
            DescentTrueAirspeed = request.DescentTrueAirspeed,
            SttFuelGals = request.SttFuelGals,
            FuelOnBoardGals = request.FuelOnBoardGals
        };
    }

    /// <summary>
    /// Creates a new entity from request with unit conversion from user units to canonical units (knots/feet).
    /// </summary>
    public static AircraftPerformanceProfile CreateFromRequest(
        string userId,
        SaveAircraftPerformanceProfileRequestDto request,
        AirspeedUnits airspeedUnits,
        LengthUnits lengthUnits)
    {
        return new AircraftPerformanceProfile
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            AircraftId = request.AircraftId,
            ProfileName = request.ProfileName,
            ClimbTrueAirspeed = AirspeedConversion.ToKnotsInt(request.ClimbTrueAirspeed, airspeedUnits),
            CruiseTrueAirspeed = AirspeedConversion.ToKnotsInt(request.CruiseTrueAirspeed, airspeedUnits),
            CruiseFuelBurn = request.CruiseFuelBurn,
            ClimbFuelBurn = request.ClimbFuelBurn,
            DescentFuelBurn = request.DescentFuelBurn,
            ClimbFpm = LengthConversion.ToFeetInt(request.ClimbFpm, lengthUnits),
            DescentFpm = LengthConversion.ToFeetInt(request.DescentFpm, lengthUnits),
            DescentTrueAirspeed = AirspeedConversion.ToKnotsInt(request.DescentTrueAirspeed, airspeedUnits),
            SttFuelGals = request.SttFuelGals,
            FuelOnBoardGals = request.FuelOnBoardGals
        };
    }

    /// <summary>
    /// Updates an entity from request without unit conversion (assumes canonical units).
    /// </summary>
    public static void UpdateFromRequest(AircraftPerformanceProfile entity, UpdateAircraftPerformanceProfileRequestDto request)
    {
        entity.AircraftId = request.AircraftId;
        entity.ProfileName = request.ProfileName;
        entity.ClimbTrueAirspeed = request.ClimbTrueAirspeed;
        entity.CruiseTrueAirspeed = request.CruiseTrueAirspeed;
        entity.CruiseFuelBurn = request.CruiseFuelBurn;
        entity.ClimbFuelBurn = request.ClimbFuelBurn;
        entity.DescentFuelBurn = request.DescentFuelBurn;
        entity.ClimbFpm = request.ClimbFpm;
        entity.DescentFpm = request.DescentFpm;
        entity.DescentTrueAirspeed = request.DescentTrueAirspeed;
        entity.SttFuelGals = request.SttFuelGals;
        entity.FuelOnBoardGals = request.FuelOnBoardGals;
    }

    /// <summary>
    /// Updates an entity from request with unit conversion from user units to canonical units (knots/feet).
    /// </summary>
    public static void UpdateFromRequest(
        AircraftPerformanceProfile entity,
        UpdateAircraftPerformanceProfileRequestDto request,
        AirspeedUnits airspeedUnits,
        LengthUnits lengthUnits)
    {
        entity.AircraftId = request.AircraftId;
        entity.ProfileName = request.ProfileName;
        entity.ClimbTrueAirspeed = AirspeedConversion.ToKnotsInt(request.ClimbTrueAirspeed, airspeedUnits);
        entity.CruiseTrueAirspeed = AirspeedConversion.ToKnotsInt(request.CruiseTrueAirspeed, airspeedUnits);
        entity.CruiseFuelBurn = request.CruiseFuelBurn;
        entity.ClimbFuelBurn = request.ClimbFuelBurn;
        entity.DescentFuelBurn = request.DescentFuelBurn;
        entity.ClimbFpm = LengthConversion.ToFeetInt(request.ClimbFpm, lengthUnits);
        entity.DescentFpm = LengthConversion.ToFeetInt(request.DescentFpm, lengthUnits);
        entity.DescentTrueAirspeed = AirspeedConversion.ToKnotsInt(request.DescentTrueAirspeed, airspeedUnits);
        entity.SttFuelGals = request.SttFuelGals;
        entity.FuelOnBoardGals = request.FuelOnBoardGals;
    }
}