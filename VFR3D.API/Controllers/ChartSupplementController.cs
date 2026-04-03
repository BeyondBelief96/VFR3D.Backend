using Microsoft.AspNetCore.Mvc;
using VFR3D.API.Authentication;
using VFR3D.API.Models;
using VFR3D.Infrastructure.Dtos;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ConditionalAuth]
public class ChartSupplementController(IChartSupplementService chartSupplementService) : ControllerBase
{
    /// <summary>
    /// Gets a pre-signed URL for a chart supplement by ICAO code or identifier
    /// </summary>
    /// <param name="icaoCodeOrIdent">ICAO code or airport identifier</param>
    /// <returns>Pre-signed URL for the chart supplement PDF</returns>
    /// <response code="200">Returns the URL to the chart supplement</response>
    /// <response code="404">If the chart supplement is not found</response>
    [HttpGet("{icaoCodeOrIdent}")]
    [ProducesResponseType(typeof(ChartSupplementUrlDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChartSupplementUrlDto>> GetChartSupplementUrl(string icaoCodeOrIdent)
    {
        var supplementUrl = await chartSupplementService.GetChartSupplementUrlByAirportCode(icaoCodeOrIdent);
        return Ok(supplementUrl);
    }
}
