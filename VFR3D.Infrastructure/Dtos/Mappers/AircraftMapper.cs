using VFR3D.Domain.Enums;
using VFR3D.Domain.Utilities.UnitConversions;
using VFR3D.Infrastructure.Dtos.Aircraft;
using VFR3D.Infrastructure.Dtos.AircraftPerformanceProfiles;
using AircraftEntity = VFR3D.Domain.Entities.Aircraft;

namespace VFR3D.Infrastructure.Dtos.Mappers;

public static class AircraftMapper
{
    /// <summary>
    /// Maps an entity to a DTO, converting from canonical units (knots/feet) to the aircraft's preferred units.
    /// </summary>
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
            // Convert from canonical units (feet) to user's preferred length units
            DefaultCruiseAltitude = entity.DefaultCruiseAltitude.HasValue
                ? LengthConversion.FromFeetInt(entity.DefaultCruiseAltitude.Value, entity.LengthUnits)
                : null,
            MaxCeiling = entity.MaxCeiling.HasValue
                ? LengthConversion.FromFeetInt(entity.MaxCeiling.Value, entity.LengthUnits)
                : null,
            // Convert from canonical units (knots) to user's preferred airspeed units
            GlideSpeed = entity.GlideSpeed.HasValue
                ? AirspeedConversion.FromKnotsInt(entity.GlideSpeed.Value, entity.AirspeedUnits)
                : null,
            GlideRatio = entity.GlideRatio,
            PerformanceProfiles = includeProfiles
                ? entity.PerformanceProfiles.Select(p =>
                    AircraftPerformanceProfileMapper.MapToDto(p, entity.AirspeedUnits, entity.LengthUnits)).ToList()
                : []
        };
    }

    /// <summary>
    /// Creates an entity from a request, converting from user's preferred units to canonical units (knots/feet).
    /// </summary>
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
            // Convert from user's preferred length units to canonical units (feet)
            DefaultCruiseAltitude = request.DefaultCruiseAltitude.HasValue
                ? LengthConversion.ToFeetInt(request.DefaultCruiseAltitude.Value, request.LengthUnits)
                : null,
            MaxCeiling = request.MaxCeiling.HasValue
                ? LengthConversion.ToFeetInt(request.MaxCeiling.Value, request.LengthUnits)
                : null,
            // Convert from user's preferred airspeed units to canonical units (knots)
            GlideSpeed = request.GlideSpeed.HasValue
                ? AirspeedConversion.ToKnotsInt(request.GlideSpeed.Value, request.AirspeedUnits)
                : null,
            GlideRatio = request.GlideRatio
        };
    }

    /// <summary>
    /// Updates an entity from a request, converting from user's preferred units to canonical units (knots/feet).
    /// </summary>
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
        // Convert from user's preferred length units to canonical units (feet)
        entity.DefaultCruiseAltitude = request.DefaultCruiseAltitude.HasValue
            ? LengthConversion.ToFeetInt(request.DefaultCruiseAltitude.Value, request.LengthUnits)
            : null;
        entity.MaxCeiling = request.MaxCeiling.HasValue
            ? LengthConversion.ToFeetInt(request.MaxCeiling.Value, request.LengthUnits)
            : null;
        // Convert from user's preferred airspeed units to canonical units (knots)
        entity.GlideSpeed = request.GlideSpeed.HasValue
            ? AirspeedConversion.ToKnotsInt(request.GlideSpeed.Value, request.AirspeedUnits)
            : null;
        entity.GlideRatio = request.GlideRatio;
    }
}
