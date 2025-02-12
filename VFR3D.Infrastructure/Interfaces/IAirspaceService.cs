using VFR3D.Infrastructure.Dtos;

namespace VFR3D.Infrastructure.Interfaces;

public interface IAirspaceService
{
    Task<IEnumerable<AirspaceDto>> GetByClasses(string[] airspaceClasses);
    Task<IEnumerable<AirspaceDto>> GetByCities(string[] cities);
    Task<IEnumerable<AirspaceDto>> GetByStates(string[] states);
    Task<IEnumerable<SpecialUseAirspaceDto>> GetByTypeCodes(string[] typeCodes);
    Task<IEnumerable<AirspaceDto>> GetByIcaoOrIdents(string[] icaoOrIdents);
}