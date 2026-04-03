using Microsoft.AspNetCore.Mvc;
using VFR3D.API.Authentication;
using VFR3D.API.Models;
using VFR3D.Infrastructure.Dtos;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ConditionalAuth]
public class MetarController(IMetarService metarService) : ControllerBase
{
    /// <summary>
    /// Gets METAR information for a specific airport
    /// </summary>
    /// <param name="icaoCodeOrIdent">ICAO code or airport identifier</param>
    /// <returns>METAR information for the specified airport</returns>
    /// <response code="200">Returns the METAR information</response>
    /// <response code="404">If the METAR or airport is not found</response>
    [HttpGet("{icaoCodeOrIdent}")]
    [ProducesResponseType(typeof(MetarDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MetarDto>> GetMetarForAirport(string icaoCodeOrIdent)
    {
        var metar = await metarService.GetMetarForAirport(icaoCodeOrIdent.ToUpperInvariant());
        return Ok(metar);
    }

    /// <summary>
    /// Gets all METARs for a specific state
    /// </summary>
    /// <param name="stateCode">Two-letter state code (e.g., TN, WA)</param>
    /// <returns>List of METARs for the specified state</returns>
    /// <response code="200">Returns the list of METARs</response>
    [HttpGet("state/{stateCode}")]
    [ProducesResponseType(typeof(IEnumerable<MetarDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MetarDto>>> GetMetarsByState(string stateCode)
    {
        var metars = await metarService.GetMetarsByState(stateCode.ToUpperInvariant());
        return Ok(metars);
    }

    /// <summary>
    /// Gets all METARs for multiple states
    /// </summary>
    /// <param name="stateCodes">Comma-separated list of two-letter state codes (e.g., TN,WA,OR)</param>
    /// <returns>List of METARs for the specified states</returns>
    /// <response code="200">Returns the list of METARs</response>
    [HttpGet("states/{stateCodes}")]
    [ProducesResponseType(typeof(IEnumerable<MetarDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MetarDto>>> GetMetarsByStates(string stateCodes)
    {
        var stateCodeArray = stateCodes.Split(',')
            .Select(s => s.Trim().ToUpperInvariant())
            .ToArray();

        var metars = await metarService.GetMetarsByStates(stateCodeArray);
        return Ok(metars);
    }
}
