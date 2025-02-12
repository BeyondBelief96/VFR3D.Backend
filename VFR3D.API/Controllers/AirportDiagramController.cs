using Amazon.SecretsManager.Model;
using Microsoft.AspNetCore.Mvc;
using VFR3D.Infrastructure.Dtos;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AirportDiagramController : ControllerBase
{
    private readonly IAirportDiagramService _airportDiagramService;
    private readonly ILogger<AirportDiagramController> _logger;

    public AirportDiagramController(
        IAirportDiagramService airportDiagramService,
        ILogger<AirportDiagramController> logger)
    {
        _airportDiagramService = airportDiagramService;
        _logger = logger;
    }

    /// <summary>
    /// Gets a pre-signed URL for an airport diagram by ICAO code or identifier
    /// </summary>
    /// <param name="icaoCodeOrIdent">ICAO code or airport identifier</param>
    /// <returns>Pre-signed URL for the airport diagram PDF</returns>
    /// <response code="200">Returns the URL to the airport diagram</response>
    /// <response code="404">If the airport diagram is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("{icaoCodeOrIdent}")]
    [ProducesResponseType(typeof(AirportDiagramUrlDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AirportDiagramUrlDto>> GetAirportDiagramUrl(string icaoCodeOrIdent)
    {
        try
        {
            var diagramUrl = await _airportDiagramService.GetAirportDiagramUrlByAirportCode(icaoCodeOrIdent);
            return Ok(diagramUrl);
        }
        catch (ResourceNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving airport diagram for {IcaoCodeOrIdent}", icaoCodeOrIdent);
            return StatusCode(500, "An error occurred while retrieving the airport diagram");
        }
    }
}