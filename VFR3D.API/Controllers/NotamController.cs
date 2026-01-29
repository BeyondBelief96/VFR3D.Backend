using Microsoft.AspNetCore.Mvc;
using VFR3D.API.Authentication;
using VFR3D.Infrastructure.Dtos.Notam;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ConditionalAuth]
public class NotamController(INotamService notamService, ILogger<NotamController> logger)
    : ControllerBase
{
    /// <summary>
    /// Gets NOTAMs for a specific airport
    /// </summary>
    /// <param name="icaoCodeOrIdent">ICAO code or FAA identifier (e.g., KDFW, DFW)</param>
    /// <returns>NOTAMs for the specified airport</returns>
    /// <response code="200">Returns the NOTAMs</response>
    /// <response code="400">If the identifier is invalid</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("{icaoCodeOrIdent}")]
    [ProducesResponseType(typeof(NotamResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<NotamResponseDto>> GetNotamsForAirport(
        string icaoCodeOrIdent,
        CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(icaoCodeOrIdent))
            {
                return BadRequest("Airport identifier is required");
            }

            var result = await notamService.GetNotamsForAirportAsync(icaoCodeOrIdent, ct);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving NOTAMs for airport {Identifier}", icaoCodeOrIdent);
            return StatusCode(500, "An error occurred while retrieving NOTAMs");
        }
    }

    /// <summary>
    /// Gets NOTAMs within a radius of a geographic point
    /// </summary>
    /// <param name="latitude">Latitude in decimal degrees</param>
    /// <param name="longitude">Longitude in decimal degrees</param>
    /// <param name="radiusNm">Radius in nautical miles (max 100)</param>
    /// <returns>NOTAMs within the specified radius</returns>
    /// <response code="200">Returns the NOTAMs</response>
    /// <response code="400">If parameters are invalid</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("radius")]
    [ProducesResponseType(typeof(NotamResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<NotamResponseDto>> GetNotamsByRadius(
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] double radiusNm,
        CancellationToken ct)
    {
        try
        {
            if (latitude < -90 || latitude > 90)
            {
                return BadRequest("Latitude must be between -90 and 90 degrees");
            }

            if (longitude < -180 || longitude > 180)
            {
                return BadRequest("Longitude must be between -180 and 180 degrees");
            }

            if (radiusNm <= 0 || radiusNm > 100)
            {
                return BadRequest("Radius must be between 0 and 100 nautical miles");
            }

            var result = await notamService.GetNotamsByRadiusAsync(latitude, longitude, radiusNm, ct);
            return Ok(result);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving NOTAMs for radius query at {Lat}, {Lon}", latitude, longitude);
            return StatusCode(500, "An error occurred while retrieving NOTAMs");
        }
    }

    /// <summary>
    /// Gets NOTAMs for a flight route (multiple airports)
    /// </summary>
    /// <param name="request">Route query request with airport identifiers</param>
    /// <returns>Aggregated and deduplicated NOTAMs for the route</returns>
    /// <response code="200">Returns the NOTAMs</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost("route")]
    [ProducesResponseType(typeof(NotamResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<NotamResponseDto>> GetNotamsForRoute(
        [FromBody] NotamQueryByRouteRequest request,
        CancellationToken ct)
    {
        try
        {
            if (request == null)
            {
                return BadRequest("Request body is required");
            }

            if (request.AirportIdentifiers == null || request.AirportIdentifiers.Count == 0)
            {
                return BadRequest("At least one airport identifier is required");
            }

            var result = await notamService.GetNotamsForRouteAsync(request, ct);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving NOTAMs for route query");
            return StatusCode(500, "An error occurred while retrieving NOTAMs");
        }
    }
}
