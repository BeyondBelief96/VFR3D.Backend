using VFR3D.Infrastructure.Dtos;

namespace VFR3D.Infrastructure.Interfaces;

public interface IAirsigmetService
{
    Task<IEnumerable<AirsigmetDto>> GetAllAirsigmets();
}