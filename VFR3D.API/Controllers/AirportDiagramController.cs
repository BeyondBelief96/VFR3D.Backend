using Microsoft.AspNetCore.Mvc;
using VFR3D.API.Authentication;
using VFR3D.API.Models;
using VFR3D.Infrastructure.Dtos;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ConditionalAuth]
public class AirportDiagramController(IAirportDiagramService airportDiagramService) : ControllerBase
{
    /// <summary>
    /// Gets pre-signed URLs for all airport diagrams by ICAO code or identifier
    /// </summary>
    /// <param name="icaoCodeOrIdent">ICAO code or airport identifier</param>
    /// <returns>Airport information with pre-signed URLs for all diagram PDFs</returns>
    /// <response code="200">Returns the airport diagrams</response>
    /// <response code="404">If no airport diagrams are found</response>
    [HttpGet("{icaoCodeOrIdent}")]
    [ProducesResponseType(typeof(AirportDiagramsResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AirportDiagramsResponseDto>> GetAirportDiagrams(string icaoCodeOrIdent)
    {
        var diagrams = await airportDiagramService.GetAirportDiagramsByAirportCode(icaoCodeOrIdent);
        return Ok(diagrams);
    }
}
