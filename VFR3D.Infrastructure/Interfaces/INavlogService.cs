using VFR3D.Infrastructure.Dtos.Navlog;

namespace VFR3D.Infrastructure.Interfaces;

public interface INavlogService
{
    Task<NavlogResponseDto> CalculateNavlog(NavlogRequestDto request);
    Task<BearingAndDistanceResponseDto> CalculateBearingAndDistance(BearingAndDistanceRequestDto request);
    Task<WindsAloftDto> GetWindsAloftData(int forecast);
}