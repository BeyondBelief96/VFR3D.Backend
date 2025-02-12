using VFR3D.Infrastructure.Dtos;

namespace VFR3D.Infrastructure.Interfaces;

public interface IAirportDiagramService
{
    Task<AirportDiagramUrlDto> GetAirportDiagramUrlByAirportCode(string airportCode);
}