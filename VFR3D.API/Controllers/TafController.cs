using Microsoft.AspNetCore.Mvc;
using VFR3D.API.Authentication;
using VFR3D.API.Models;
using VFR3D.Infrastructure.Dtos;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ConditionalAuth]
public class TafController(ITafService tafService) : ControllerBase
{
    /// <summary>
    /// Gets TAF information for a specific airport
    /// </summary>
    /// <param name="icaoCodeOrIdent">ICAO code or airport identifier</param>
    /// <returns>TAF information for the specified airport</returns>
    /// <response code="200">Returns the TAF information</response>
    /// <response code="404">If the TAF or airport is not found</response>
    [HttpGet("{icaoCodeOrIdent}")]
    [ProducesResponseType(typeof(TafDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TafDto>> GetTafByIcaoCodeOrIdent(string icaoCodeOrIdent)
    {
        var taf = await tafService.GetTafByIcaoCode(icaoCodeOrIdent.ToUpperInvariant());
        return Ok(taf);
    }
}
