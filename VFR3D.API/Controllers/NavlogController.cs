using Microsoft.AspNetCore.Mvc;
using VFR3D.Infrastructure.Dtos.Navlog;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NavlogController : ControllerBase
{
    private readonly INavlogService _navlogService;
    private readonly ILogger<NavlogController> _logger;

    public NavlogController(
        INavlogService navlogService,
        ILogger<NavlogController> logger)
    {
        _navlogService = navlogService;
        _logger = logger;
    }

    /// <summary>
    /// Calculates a complete navigation log for a flight
    /// </summary>
    /// <param name="request">Navigation log request including waypoints and aircraft performance settings</param>
    /// <returns>Complete navigation log with leg calculations</returns>
    /// <response code="200">Returns the calculated navigation log</response>
    /// <response code="400">If the request data is invalid</response>
    /// <response code="404">If the aircraft performance profile is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("[action]")]
    [ProducesResponseType(typeof(NavlogResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<NavlogResponseDto>> CalculateNavlog([FromBody] NavlogRequestDto request)
    {
        try
        {
            _logger.LogInformation("Calculating navlog for {WaypointCount} waypoints", request.Waypoints.Count);
            var response = await _navlogService.CalculateNavlog(request);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid navlog request data");
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Aircraft performance profile not found");
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating navlog");
            return StatusCode(500, "An error occurred while calculating the navigation log");
        }
    }

    /// <summary>
    /// Calculates bearing and distance between two points
    /// </summary>
    /// <param name="request">Start and end points for the calculation</param>
    /// <returns>True course, magnetic course, and distance between the points</returns>
    /// <response code="200">Returns the bearing and distance calculation</response>
    /// <response code="400">If the request data is invalid</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("[action]")]
    [ProducesResponseType(typeof(BearingAndDistanceResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BearingAndDistanceResponseDto>> CalculateBearingAndDistance(
        [FromBody] BearingAndDistanceRequestDto request)
    {
        try
        {
            _logger.LogInformation("Calculating bearing and distance between points");
            var response = await _navlogService.CalculateBearingAndDistance(request);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid bearing and distance request data");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating bearing and distance");
            return StatusCode(500, "An error occurred while calculating bearing and distance");
        }
    }

    /// <summary>
    /// Gets winds aloft data for a specific forecast period
    /// </summary>
    /// <param name="forecast">Forecast period (6, 12, or 24 hours)</param>
    /// <returns>Winds aloft data for the specified forecast period</returns>
    /// <response code="200">Returns the winds aloft data</response>
    /// <response code="400">If the forecast period is invalid</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("[action]/{forecast}")]
    [ProducesResponseType(typeof(WindsAloftDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<WindsAloftDto>> GetWindsAloftData(int forecast)
    {
        try
        {
            if (forecast != 6 && forecast != 12 && forecast != 24)
            {
                return BadRequest("Forecast period must be 6, 12, or 24 hours");
            }

            _logger.LogInformation("Getting winds aloft data for {Forecast} hour forecast", forecast);
            var response = await _navlogService.GetWindsAloftData(forecast);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting winds aloft data");
            return StatusCode(500, "An error occurred while retrieving winds aloft data");
        }
    }
}