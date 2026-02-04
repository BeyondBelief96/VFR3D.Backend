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
    /// Gets a pre-signed URL for an airport diagram by ICAO code or identifier
    /// </summary>
    /// <param name="icaoCodeOrIdent">ICAO code or airport identifier</param>
    /// <returns>Pre-signed URL for the airport diagram PDF</returns>
    /// <response code="200">Returns the URL to the airport diagram</response>
    /// <response code="404">If the airport diagram is not found</response>
    [HttpGet("{icaoCodeOrIdent}")]
    [ProducesResponseType(typeof(AirportDiagramUrlDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AirportDiagramUrlDto>> GetAirportDiagramUrl(string icaoCodeOrIdent)
    {
        var diagramUrl = await airportDiagramService.GetAirportDiagramUrlByAirportCode(icaoCodeOrIdent);
        return Ok(diagramUrl);
    }
}
