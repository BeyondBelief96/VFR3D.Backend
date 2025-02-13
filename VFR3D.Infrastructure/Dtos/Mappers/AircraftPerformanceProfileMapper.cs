using VFR3D.Domain.Entities;
using VFR3D.Infrastructure.Dtos.AircraftPerformanceProfiles;

namespace VFR3D.Infrastructure.Dtos.Mappers;

public static class AircraftPerformanceProfileMapper
{
    public static AircraftPerformanceProfileDto MapToDto(AircraftPerformanceProfile entity)
    {
        return new AircraftPerformanceProfileDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
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
            FuelOnBoardGals = entity.FuelOnBoardGals
        };
    }

    public static AircraftPerformanceProfile MapToEntity(AircraftPerformanceProfileDto dto)
    {
        return new AircraftPerformanceProfile
        {
            Id = dto.Id,
            UserId = dto.UserId,
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

    public static AircraftPerformanceProfile CreateFromRequest(string userId, SaveAircraftPerformanceProfileRequestDto request)
    {
        return new AircraftPerformanceProfile
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
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

    public static void UpdateFromRequest(AircraftPerformanceProfile entity, UpdateAircraftPerformanceProfileRequestDto request)
    {
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
}