using Microsoft.AspNetCore.Mvc;
using VFR3D.API.Authentication;
using VFR3D.API.Models;
using VFR3D.Domain.Exceptions;
using VFR3D.Infrastructure.Dtos.Navlog;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ConditionalAuth]
public class NavlogController(INavlogService navlogService)
    : ControllerBase
{
    /// <summary>
    /// Calculates a complete navigation log for a flight
    /// </summary>
    /// <param name="request">Navigation log request including waypoints and aircraft performance settings</param>
    /// <returns>Complete navigation log with leg calculations</returns>
    /// <response code="200">Returns the calculated navigation log</response>
    /// <response code="400">If the request data is invalid</response>
    /// <response code="404">If the aircraft performance profile is not found</response>
    [HttpPost("[action]")]
    [ProducesResponseType(typeof(NavlogResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NavlogResponseDto>> CalculateNavlog([FromBody] NavlogRequestDto request)
    {
        var response = await navlogService.CalculateNavlog(request);
        return Ok(response);
    }

    /// <summary>
    /// Calculates bearing and distance between two points
    /// </summary>
    /// <param name="request">Start and end points for the calculation</param>
    /// <returns>True course, magnetic course, and distance between the points</returns>
    /// <response code="200">Returns the bearing and distance calculation</response>
    /// <response code="400">If the request data is invalid</response>
    [HttpPost("[action]")]
    [ProducesResponseType(typeof(BearingAndDistanceResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BearingAndDistanceResponseDto>> CalculateBearingAndDistance(
        [FromBody] BearingAndDistanceRequestDto request)
    {
        var response = await navlogService.CalculateBearingAndDistance(request);
        return Ok(response);
    }

    /// <summary>
    /// Gets winds aloft data for a specific forecast period
    /// </summary>
    /// <param name="forecast">Forecast period (6, 12, or 24 hours)</param>
    /// <returns>Winds aloft data for the specified forecast period</returns>
    /// <response code="200">Returns the winds aloft data</response>
    /// <response code="400">If the forecast period is invalid</response>
    [HttpGet("[action]/{forecast}")]
    [ProducesResponseType(typeof(WindsAloftDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WindsAloftDto>> GetWindsAloftData(int forecast)
    {
        if (forecast != 6 && forecast != 12 && forecast != 24)
            throw new ValidationException("Forecast", "Forecast period must be 6, 12, or 24 hours");

        var response = await navlogService.GetWindsAloftData(forecast);
        return Ok(response);
    }
}
