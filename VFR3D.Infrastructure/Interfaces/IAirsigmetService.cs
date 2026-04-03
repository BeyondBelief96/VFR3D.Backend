using VFR3D.Domain.Enums;
using VFR3D.Infrastructure.Dtos;

namespace VFR3D.Infrastructure.Interfaces;

public interface IAirsigmetService
{
    Task<IEnumerable<AirsigmetDto>> GetAllAirsigmets();
    Task<IEnumerable<AirsigmetDto>> GetAirsigmetsByHazardType(AirsigmetHazardType hazardType);
}