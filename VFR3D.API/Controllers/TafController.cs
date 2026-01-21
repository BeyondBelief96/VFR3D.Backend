using Microsoft.AspNetCore.Mvc;
using VFR3D.API.Authentication;
using VFR3D.Domain.Exceptions;
using VFR3D.Infrastructure.Dtos;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ConditionalAuth]
public class TafController(ITafService tafService, ILogger<TafController> logger) : ControllerBase
{
    /// <summary>
    /// Gets TAF information for a specific airport
    /// </summary>
    /// <param name="icaoCodeOrIdent">ICAO code or airport identifier</param>
    /// <returns>TAF information for the specified airport</returns>
    /// <response code="200">Returns the TAF information</response>
    /// <response code="404">If the TAF or airport is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("{icaoCodeOrIdent}")]
    [ProducesResponseType(typeof(TafDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TafDto>> GetTafByIcaoCodeOrIdent(string icaoCodeOrIdent)
    {
        try
        {
            var taf = await tafService.GetTafByIcaoCode(icaoCodeOrIdent.ToUpperInvariant());
            return Ok(taf);
        }
        catch (ResourceNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving TAF for airport {IcaoCodeOrIdent}", icaoCodeOrIdent);
            return StatusCode(500, "An error occurred while retrieving the TAF information");
        }
    }
}