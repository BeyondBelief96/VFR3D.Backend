using VFR3D.Infrastructure.Dtos;

namespace VFR3D.Infrastructure.Interfaces;

public interface IAirportDiagramService
{
    Task<AirportDiagramsResponseDto> GetAirportDiagramsByAirportCode(string airportCode);
}