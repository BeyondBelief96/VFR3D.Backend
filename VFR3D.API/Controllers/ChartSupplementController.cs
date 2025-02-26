using Amazon.SecretsManager.Model;
using Microsoft.AspNetCore.Mvc;
using VFR3D.API.Authentication;
using VFR3D.Infrastructure.Dtos;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ConditionalAuth]
public class ChartSupplementController(
    IChartSupplementService chartSupplementService,
    ILogger<ChartSupplementController> logger)
    : ControllerBase
{
    /// <summary>
    /// Gets a pre-signed URL for a chart supplement by ICAO code or identifier
    /// </summary>
    /// <param name="icaoCodeOrIdent">ICAO code or airport identifier</param>
    /// <returns>Pre-signed URL for the chart supplement PDF</returns>
    /// <response code="200">Returns the URL to the chart supplement</response>
    /// <response code="404">If the chart supplement is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("{icaoCodeOrIdent}")]
    [ProducesResponseType(typeof(ChartSupplementUrlDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ChartSupplementUrlDto>> GetChartSupplementUrl(string icaoCodeOrIdent)
    {
        try
        {
            var supplementUrl = await chartSupplementService.GetChartSupplementUrlByAirportCode(icaoCodeOrIdent);
            return Ok(supplementUrl);
        }
        catch (ResourceNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving chart supplement for {IcaoCodeOrIdent}", icaoCodeOrIdent);
            return StatusCode(500, "An error occurred while retrieving the chart supplement");
        }
    }
}