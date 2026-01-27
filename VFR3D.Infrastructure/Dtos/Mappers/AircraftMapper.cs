using VFR3D.Infrastructure.Dtos.Aircraft;
using VFR3D.Infrastructure.Dtos.AircraftPerformanceProfiles;
using AircraftEntity = VFR3D.Domain.Entities.Aircraft;

namespace VFR3D.Infrastructure.Dtos.Mappers;

public static class AircraftMapper
{
    public static AircraftDto MapToDto(AircraftEntity entity, bool includeProfiles = false)
    {
        return new AircraftDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            TailNumber = entity.TailNumber,
            AircraftType = entity.AircraftType,
            CallSign = entity.CallSign,
            SerialNumber = entity.SerialNumber,
            PrimaryColor = entity.PrimaryColor,
            Color2 = entity.Color2,
            Color3 = entity.Color3,
            Color4 = entity.Color4,
            Category = entity.Category,
            AircraftHome = entity.AircraftHome,
            AirspeedUnits = entity.AirspeedUnits,
            LengthUnits = entity.LengthUnits,
            DefaultCruiseAltitude = entity.DefaultCruiseAltitude,
            MaxCeiling = entity.MaxCeiling,
            GlideSpeed = entity.GlideSpeed,
            GlideRatio = entity.GlideRatio,
            PerformanceProfiles = includeProfiles
                ? entity.PerformanceProfiles.Select(AircraftPerformanceProfileMapper.MapToDto).ToList()
                : []
        };
    }

    public static AircraftEntity CreateFromRequest(string userId, CreateAircraftRequestDto request)
    {
        return new AircraftEntity
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            TailNumber = request.TailNumber,
            AircraftType = request.AircraftType,
            CallSign = request.CallSign,
            SerialNumber = request.SerialNumber,
            PrimaryColor = request.PrimaryColor,
            Color2 = request.Color2,
            Color3 = request.Color3,
            Color4 = request.Color4,
            Category = request.Category,
            AircraftHome = request.AircraftHome,
            AirspeedUnits = request.AirspeedUnits,
            LengthUnits = request.LengthUnits,
            DefaultCruiseAltitude = request.DefaultCruiseAltitude,
            MaxCeiling = request.MaxCeiling,
            GlideSpeed = request.GlideSpeed,
            GlideRatio = request.GlideRatio
        };
    }

    public static void UpdateFromRequest(AircraftEntity entity, UpdateAircraftRequestDto request)
    {
        entity.TailNumber = request.TailNumber;
        entity.AircraftType = request.AircraftType;
        entity.CallSign = request.CallSign;
        entity.SerialNumber = request.SerialNumber;
        entity.PrimaryColor = request.PrimaryColor;
        entity.Color2 = request.Color2;
        entity.Color3 = request.Color3;
        entity.Color4 = request.Color4;
        entity.Category = request.Category;
        entity.AircraftHome = request.AircraftHome;
        entity.AirspeedUnits = request.AirspeedUnits;
        entity.LengthUnits = request.LengthUnits;
        entity.DefaultCruiseAltitude = request.DefaultCruiseAltitude;
        entity.MaxCeiling = request.MaxCeiling;
        entity.GlideSpeed = request.GlideSpeed;
        entity.GlideRatio = request.GlideRatio;
    }
}
