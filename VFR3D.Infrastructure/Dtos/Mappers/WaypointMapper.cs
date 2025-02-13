using VFR3D.Domain.ValueObjects.Flights;
using VFR3D.Infrastructure.Dtos.Navlog;

namespace VFR3D.Infrastructure.Dtos.Mappers;

public static class WaypointMapper
{
    public static Waypoint MapToEntity(WaypointDto dto)
    {
        return new Waypoint
        {
            Id = dto.Id,
            Name = dto.Name,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            Altitude = dto.Altitude,
            WaypointType = dto.WaypointType
        };
    }

    public static WaypointDto MapToDto(Waypoint entity)
    {
        return new WaypointDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Latitude = entity.Latitude,
            Longitude = entity.Longitude,
            Altitude = entity.Altitude,
            WaypointType = entity.WaypointType
        };
    }
}