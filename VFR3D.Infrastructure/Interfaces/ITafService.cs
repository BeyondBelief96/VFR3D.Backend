using VFR3D.Infrastructure.Dtos;

namespace VFR3D.Infrastructure.Interfaces;

public interface ITafService
{
    Task<TafDto> GetTafByIcaoCode(string icaoCodeOrIdent);
}