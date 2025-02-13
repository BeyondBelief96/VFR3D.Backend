using VFR3D.Domain.ValueObjects.Flights;
using VFR3D.Infrastructure.Dtos.Navlog;

namespace VFR3D.Infrastructure.Dtos.Mappers;

public static class NavlogLegMapper
{
    public static NavlogLeg MapToEntity(NavigationLegDto dto)
    {
        return new NavlogLeg
        {
            LegStartPoint = WaypointMapper.MapToEntity(dto.LegStartPoint),
            LegEndPoint = WaypointMapper.MapToEntity(dto.LegEndPoint),
            TrueCourse = dto.TrueCourse,
            MagneticHeading = dto.MagneticHeading,
            MagneticCourse = dto.MagneticCourse,
            GroundSpeed = dto.GroundSpeed,
            LegDistance = dto.LegDistance,
            DistanceRemaining = dto.DistanceRemaining,
            StartLegTime = dto.StartLegTime,
            EndLegTime = dto.EndLegTime,
            LegFuelBurnGals = dto.LegFuelBurnGals,
            RemainingFuelGals = dto.RemainingFuelGals,
            WindDir = dto.WindDir,
            WindSpeed = dto.WindSpeed,
            HeadwindComponent = dto.HeadwindComponent,
            TempC = dto.TempC
        };
    }

    public static NavigationLegDto MapToDto(NavlogLeg entity)
    {
        return new NavigationLegDto
        {
            LegStartPoint = WaypointMapper.MapToDto(entity.LegStartPoint),
            LegEndPoint = WaypointMapper.MapToDto(entity.LegEndPoint),
            TrueCourse = entity.TrueCourse,
            MagneticHeading = entity.MagneticHeading,
            MagneticCourse = entity.MagneticCourse,
            GroundSpeed = entity.GroundSpeed,
            LegDistance = entity.LegDistance,
            DistanceRemaining = entity.DistanceRemaining,
            StartLegTime = entity.StartLegTime,
            EndLegTime = entity.EndLegTime,
            LegFuelBurnGals = entity.LegFuelBurnGals,
            RemainingFuelGals = entity.RemainingFuelGals,
            WindDir = entity.WindDir,
            WindSpeed = entity.WindSpeed,
            HeadwindComponent = entity.HeadwindComponent,
            TempC = entity.TempC
        };
    }
}