using Amazon.SecretsManager.Model;
using Microsoft.AspNetCore.Mvc;
using VFR3D.Infrastructure.Dtos;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TafController : ControllerBase
{
    private readonly ITafService _tafService;
    private readonly ILogger<TafController> _logger;

    public TafController(ITafService tafService, ILogger<TafController> logger)
    {
        _tafService = tafService;
        _logger = logger;
    }

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
            var taf = await _tafService.GetTafByIcaoCode(icaoCodeOrIdent.ToUpperInvariant());
            return Ok(taf);
        }
        catch (ResourceNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving TAF for airport {IcaoCodeOrIdent}", icaoCodeOrIdent);
            return StatusCode(500, "An error occurred while retrieving the TAF information");
        }
    }
}