using Microsoft.AspNetCore.Mvc;
using VFR3D.API.Authentication;
using VFR3D.API.Models;
using VFR3D.Infrastructure.Dtos.Performance;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ConditionalAuth]
public class PerformanceController(IPerformanceCalculatorService performanceCalculatorService)
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
    [HttpGet("crosswind/{icaoCodeOrIdent}")]
    [ProducesResponseType(typeof(AirportCrosswindResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AirportCrosswindResponseDto>> GetCrosswindForAirport(string icaoCodeOrIdent)
    {
        var result = await performanceCalculatorService.GetCrosswindForAirportAsync(icaoCodeOrIdent);
        return Ok(result);
    }

    /// <summary>
    /// Calculates crosswind components using manual parameters
    /// </summary>
    /// <param name="request">Wind and runway heading parameters</param>
    /// <returns>Calculated crosswind and headwind components</returns>
    /// <response code="200">Returns calculated crosswind components</response>
    /// <response code="400">If the request parameters are invalid</response>
    [HttpPost("crosswind/calculate")]
    [ProducesResponseType(typeof(CrosswindCalculationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public ActionResult<CrosswindCalculationResponseDto> CalculateCrosswind(
        [FromBody] CrosswindCalculationRequestDto request)
    {
        var result = performanceCalculatorService.CalculateCrosswind(request);
        return Ok(result);
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
    [HttpGet("density-altitude/{icaoCodeOrIdent}")]
    [ProducesResponseType(typeof(DensityAltitudeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DensityAltitudeResponseDto>> GetDensityAltitudeForAirport(
        string icaoCodeOrIdent,
        [FromQuery] AirportDensityAltitudeRequestDto? request = null)
    {
        var result = await performanceCalculatorService.GetDensityAltitudeForAirportAsync(
            icaoCodeOrIdent, request);
        return Ok(result);
    }

    /// <summary>
    /// Calculates density altitude using manual parameters
    /// </summary>
    /// <param name="request">Field elevation, altimeter, and temperature</param>
    /// <returns>Calculated density altitude with pressure altitude and ISA deviation</returns>
    /// <response code="200">Returns calculated density altitude</response>
    /// <response code="400">If the request parameters are invalid</response>
    [HttpPost("density-altitude/calculate")]
    [ProducesResponseType(typeof(DensityAltitudeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public ActionResult<DensityAltitudeResponseDto> CalculateDensityAltitude(
        [FromBody] DensityAltitudeRequestDto request)
    {
        var result = performanceCalculatorService.CalculateDensityAltitude(request);
        return Ok(result);
    }
}
