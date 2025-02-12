using VFR3D.Infrastructure.Dtos;

namespace VFR3D.Infrastructure.Interfaces;

public interface IChartSupplementService
{
    Task<ChartSupplementUrlDto> GetChartSupplementUrlByAirportCode(string icaoCodeOrIdent);
}