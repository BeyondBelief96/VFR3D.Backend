using VFR3D.Infrastructure.Dtos.Navlog;

namespace VFR3D.Infrastructure.Interfaces;

public interface IPreflightApiClient
{
    Task<NavlogResponseDto> CalculateNavlogAsync(
        NavlogRequestDto request,
        NavlogPerformanceDataDto performanceData);
}
