using VFR3D.Infrastructure.Dtos.Navlog;

namespace VFR3D.Infrastructure.Interfaces;

public interface IWindsAloftService
{
    Task<WindsAloftDto> FetchWindsAloftData(int fcstHours);
}