using Amazon.SecretsManager.Model;
using Microsoft.AspNetCore.Mvc;
using VFR3D.API.Authentication;
using VFR3D.Infrastructure.Dtos;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ConditionalAuth]
public class MetarController : ControllerBase
{
    private readonly IMetarService _metarService;
    private readonly ILogger<MetarController> _logger;

    public MetarController(IMetarService metarService, ILogger<MetarController> logger)
    {
        _metarService = metarService;
        _logger = logger;
    }

    /// <summary>
    /// Gets METAR information for a specific airport
    /// </summary>
    /// <param name="icaoCodeOrIdent">ICAO code or airport identifier</param>
    /// <returns>METAR information for the specified airport</returns>
    /// <response code="200">Returns the METAR information</response>
    /// <response code="404">If the METAR or airport is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("{icaoCodeOrIdent}")]
    [ProducesResponseType(typeof(MetarDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MetarDto>> GetMetarForAirport(string icaoCodeOrIdent)
    {
        try
        {
            var metar = await _metarService.GetMetarForAirport(icaoCodeOrIdent.ToUpperInvariant());
            return Ok(metar);
        }
        catch (ResourceNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving METAR for airport {IcaoCodeOrIdent}", icaoCodeOrIdent);
            return StatusCode(500, "An error occurred while retrieving the METAR information");
        }
    }

    /// <summary>
    /// Gets all METARs for a specific state
    /// </summary>
    /// <param name="stateCode">Two-letter state code (e.g., TN, WA)</param>
    /// <returns>List of METARs for the specified state</returns>
    /// <response code="200">Returns the list of METARs</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("state/{stateCode}")]
    [ProducesResponseType(typeof(IEnumerable<MetarDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<MetarDto>>> GetMetarsByState(string stateCode)
    {
        try
        {
            var metars = await _metarService.GetMetarsByState(stateCode.ToUpperInvariant());
            return Ok(metars);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving METARs for state {StateCode}", stateCode);
            return StatusCode(500, "An error occurred while retrieving the METAR information");
        }
    }

    /// <summary>
    /// Gets all METARs for multiple states
    /// </summary>
    /// <param name="stateCodes">Comma-separated list of two-letter state codes (e.g., TN,WA,OR)</param>
    /// <returns>List of METARs for the specified states</returns>
    /// <response code="200">Returns the list of METARs</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("states/{stateCodes}")]
    [ProducesResponseType(typeof(IEnumerable<MetarDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<MetarDto>>> GetMetarsByStates(string stateCodes)
    {
        try
        {
            var stateCodeArray = stateCodes.Split(',')
                .Select(s => s.Trim().ToUpperInvariant())
                .ToArray();

            var metars = await _metarService.GetMetarsByStates(stateCodeArray);
            return Ok(metars);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving METARs for states {StateCodes}", stateCodes);
            return StatusCode(500, "An error occurred while retrieving the METAR information");
        }
    }
}