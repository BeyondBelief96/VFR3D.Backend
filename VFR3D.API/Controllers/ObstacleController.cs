using Microsoft.AspNetCore.Mvc;
using VFR3D.API.Authentication;
using VFR3D.Infrastructure.Dtos;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ConditionalAuth]
public class ObstacleController(
    IObstacleService obstacleService,
    ILogger<ObstacleController> logger)
    : ControllerBase
{
    /// <summary>
    /// Searches for obstacles near a coordinate
    /// </summary>
    /// <param name="lat">Latitude in decimal degrees</param>
    /// <param name="lon">Longitude in decimal degrees</param>
    /// <param name="radiusNm">Search radius in nautical miles (default: 5)</param>
    /// <param name="minHeightAgl">Minimum height AGL in feet (optional filter)</param>
    /// <param name="limit">Maximum number of results (default: 100, max: 500)</param>
    /// <returns>List of obstacles sorted by height AMSL descending</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<ObstacleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ObstacleDto>>> SearchNearby(
        [FromQuery] decimal lat,
        [FromQuery] decimal lon,
        [FromQuery] double radiusNm = 5,
        [FromQuery] int? minHeightAgl = null,
        [FromQuery] int limit = 100)
    {
        try
        {
            limit = Math.Min(limit, 500);
            var obstacles = await obstacleService.SearchNearby(lat, lon, radiusNm, minHeightAgl, limit);
            return Ok(obstacles);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching obstacles near ({Lat}, {Lon})", lat, lon);
            return StatusCode(500, "An error occurred while searching for obstacles");
        }
    }

    /// <summary>
    /// Gets obstacles by state code
    /// </summary>
    /// <param name="stateCode">Two-letter state code (e.g., CO, CA, TX)</param>
    /// <param name="minHeightAgl">Minimum height AGL in feet (optional filter)</param>
    /// <param name="limit">Maximum number of results (default: 1000, max: 5000)</param>
    /// <returns>List of obstacles in the state sorted by height AMSL descending</returns>
    [HttpGet("state/{stateCode}")]
    [ProducesResponseType(typeof(IEnumerable<ObstacleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ObstacleDto>>> GetByState(
        string stateCode,
        [FromQuery] int? minHeightAgl = null,
        [FromQuery] int limit = 1000)
    {
        try
        {
            limit = Math.Min(limit, 5000);
            var obstacles = await obstacleService.GetByState(stateCode, minHeightAgl, limit);
            return Ok(obstacles);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting obstacles for state: {StateCode}", stateCode);
            return StatusCode(500, "An error occurred while retrieving obstacles");
        }
    }

    /// <summary>
    /// Gets an obstacle by its OAS number
    /// </summary>
    /// <param name="oasNumber">OAS number (e.g., 08-000001)</param>
    /// <returns>Obstacle details</returns>
    [HttpGet("{oasNumber}")]
    [ProducesResponseType(typeof(ObstacleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ObstacleDto>> GetByOasNumber(string oasNumber)
    {
        try
        {
            var obstacle = await obstacleService.GetByOasNumber(oasNumber);
            if (obstacle == null)
            {
                return NotFound($"Obstacle not found: {oasNumber}");
            }
            return Ok(obstacle);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting obstacle: {OasNumber}", oasNumber);
            return StatusCode(500, "An error occurred while retrieving the obstacle");
        }
    }

    /// <summary>
    /// Gets multiple obstacles by their OAS numbers
    /// </summary>
    /// <param name="oasNumbers">List of OAS numbers</param>
    /// <returns>List of obstacles sorted by height AMSL descending</returns>
    [HttpPost("by-oas-numbers")]
    [ProducesResponseType(typeof(IEnumerable<ObstacleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ObstacleDto>>> GetByOasNumbers([FromBody] List<string> oasNumbers)
    {
        try
        {
            if (oasNumbers == null || oasNumbers.Count == 0)
            {
                return BadRequest("At least one OAS number is required");
            }

            if (oasNumbers.Count > 1000)
            {
                return BadRequest("Maximum of 1000 OAS numbers allowed per request");
            }

            var obstacles = await obstacleService.GetByOasNumbers(oasNumbers);
            return Ok(obstacles);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting obstacles by OAS numbers");
            return StatusCode(500, "An error occurred while retrieving obstacles");
        }
    }

    /// <summary>
    /// Gets obstacles within a bounding box
    /// </summary>
    /// <param name="minLat">Minimum latitude</param>
    /// <param name="maxLat">Maximum latitude</param>
    /// <param name="minLon">Minimum longitude</param>
    /// <param name="maxLon">Maximum longitude</param>
    /// <param name="minHeightAgl">Minimum height AGL in feet (optional filter)</param>
    /// <param name="limit">Maximum number of results (default: 1000, max: 5000)</param>
    /// <returns>List of obstacles in the bounding box sorted by height AMSL descending</returns>
    [HttpGet("bbox")]
    [ProducesResponseType(typeof(IEnumerable<ObstacleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ObstacleDto>>> GetByBoundingBox(
        [FromQuery] decimal minLat,
        [FromQuery] decimal maxLat,
        [FromQuery] decimal minLon,
        [FromQuery] decimal maxLon,
        [FromQuery] int? minHeightAgl = null,
        [FromQuery] int limit = 1000)
    {
        try
        {
            limit = Math.Min(limit, 5000);
            var obstacles = await obstacleService.GetByBoundingBox(minLat, maxLat, minLon, maxLon, minHeightAgl, limit);
            return Ok(obstacles);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting obstacles in bounding box");
            return StatusCode(500, "An error occurred while retrieving obstacles");
        }
    }
}
