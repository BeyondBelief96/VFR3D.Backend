using VFR3D.Domain.Enums;
using VFR3D.Infrastructure.Dtos;

namespace VFR3D.Infrastructure.Interfaces;

public interface IGAirmetService
{
    Task<IEnumerable<GAirmetDto>> GetAllGAirmets();
    Task<IEnumerable<GAirmetDto>> GetGAirmetsByProduct(GAirmetProduct product);
    Task<IEnumerable<GAirmetDto>> GetGAirmetsByHazardType(GAirmetHazardType hazardType);
}
