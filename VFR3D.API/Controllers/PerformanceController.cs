using Microsoft.AspNetCore.Mvc;
using VFR3D.API.Authentication;
using VFR3D.Domain.Exceptions;
using VFR3D.Infrastructure.Dtos.Performance;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ConditionalAuth]
public class PerformanceController(
    IPerformanceCalculatorService performanceCalculatorService,
    ILogger<PerformanceController> logger)
    : ControllerBase
{
    /// <summary>
    /// Calculates crosswind components for all runways at an airport using current METAR data
    /// </summary>
    /// <param name="icaoCodeOrIdent">ICAO code or airport identifier</param>
    /// <returns>Crosswind data for all runway ends with recommended runway</returns>
    /// <response code="200">Returns crosswind data for all runways</response>
    /// <response code="400">If METAR is missing required wind data</response>
    /// <response code="404">If the airport or METAR is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("crosswind/{icaoCodeOrIdent}")]
    [ProducesResponseType(typeof(AirportCrosswindResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AirportCrosswindResponseDto>> GetCrosswindForAirport(string icaoCodeOrIdent)
    {
        try
        {
            var result = await performanceCalculatorService.GetCrosswindForAirportAsync(icaoCodeOrIdent);
            return Ok(result);
        }
        catch (ResourceNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidDataException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calculating crosswind for airport: {IcaoCodeOrIdent}", icaoCodeOrIdent);
            return StatusCode(500, "An error occurred while calculating crosswind");
        }
    }

    /// <summary>
    /// Calculates crosswind components using manual parameters
    /// </summary>
    /// <param name="request">Wind and runway heading parameters</param>
    /// <returns>Calculated crosswind and headwind components</returns>
    /// <response code="200">Returns calculated crosswind components</response>
    /// <response code="400">If the request parameters are invalid</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("crosswind/calculate")]
    [ProducesResponseType(typeof(CrosswindCalculationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<CrosswindCalculationResponseDto> CalculateCrosswind(
        [FromBody] CrosswindCalculationRequestDto request)
    {
        try
        {
            var result = performanceCalculatorService.CalculateCrosswind(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calculating crosswind with manual parameters");
            return StatusCode(500, "An error occurred while calculating crosswind");
        }
    }

    /// <summary>
    /// Calculates density altitude for an airport using current METAR data
    /// </summary>
    /// <param name="icaoCodeOrIdent">ICAO code or airport identifier</param>
    /// <param name="request">Optional temperature and altimeter overrides</param>
    /// <returns>Density altitude calculation with pressure altitude and ISA deviation</returns>
    /// <response code="200">Returns density altitude data</response>
    /// <response code="400">If METAR is missing required data and no override provided</response>
    /// <response code="404">If the airport or METAR is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("density-altitude/{icaoCodeOrIdent}")]
    [ProducesResponseType(typeof(DensityAltitudeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DensityAltitudeResponseDto>> GetDensityAltitudeForAirport(
        string icaoCodeOrIdent,
        [FromQuery] AirportDensityAltitudeRequestDto? request = null)
    {
        try
        {
            var result = await performanceCalculatorService.GetDensityAltitudeForAirportAsync(
                icaoCodeOrIdent, request);
            return Ok(result);
        }
        catch (ResourceNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidDataException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calculating density altitude for airport: {IcaoCodeOrIdent}", icaoCodeOrIdent);
            return StatusCode(500, "An error occurred while calculating density altitude");
        }
    }

    /// <summary>
    /// Calculates density altitude using manual parameters
    /// </summary>
    /// <param name="request">Field elevation, altimeter, and temperature</param>
    /// <returns>Calculated density altitude with pressure altitude and ISA deviation</returns>
    /// <response code="200">Returns calculated density altitude</response>
    /// <response code="400">If the request parameters are invalid</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("density-altitude/calculate")]
    [ProducesResponseType(typeof(DensityAltitudeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<DensityAltitudeResponseDto> CalculateDensityAltitude(
        [FromBody] DensityAltitudeRequestDto request)
    {
        try
        {
            var result = performanceCalculatorService.CalculateDensityAltitude(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calculating density altitude with manual parameters");
            return StatusCode(500, "An error occurred while calculating density altitude");
        }
    }
}
