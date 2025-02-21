using Microsoft.AspNetCore.Mvc;
using VFR3D.API.Authentication;
using VFR3D.Infrastructure.Dtos.Flights;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ConditionalAuth]
public class FlightController : ControllerBase
{
    private readonly IFlightService _flightService;
    private readonly ILogger<FlightController> _logger;

    public FlightController(
        IFlightService flightService,
        ILogger<FlightController> logger)
    {
        _flightService = flightService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all flights for a user
    /// </summary>
    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(List<FlightDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<FlightDto>>> GetFlights(string userId)
    {
        try
        {
            var flights = await _flightService.GetFlights(userId);
            return Ok(flights);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting flights for user {UserId}", userId);
            return StatusCode(500, "An error occurred while retrieving flights");
        }
    }

    /// <summary>
    /// Gets a specific flight by ID
    /// </summary>
    [HttpGet("{userId}/{flightId}")]
    [ProducesResponseType(typeof(FlightDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<FlightDto>> GetFlight(string userId, string flightId)
    {
        try
        {
            var flight = await _flightService.GetFlight(userId, flightId);
            return Ok(flight);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting flight {FlightId} for user {UserId}", flightId, userId);
            return StatusCode(500, "An error occurred while retrieving the flight");
        }
    }

    /// <summary>
    /// Creates a new flight
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(FlightDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<FlightDto>> CreateFlight(
        string userId,
        [FromBody] CreateFlightRequestDto request)
    {
        try
        {
            var flight = await _flightService.CreateFlight(userId, request);
            return Ok(flight);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating flight for user {UserId}", userId);
            return StatusCode(500, "An error occurred while creating the flight");
        }
    }

    /// <summary>
    /// Updates an existing flight
    /// </summary>
    [HttpPatch("{userId}/{flightId}")]
    [ProducesResponseType(typeof(FlightDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<FlightDto>> UpdateFlight(
        string userId,
        string flightId,
        [FromBody] UpdateFlightRequestDto request)
    {
        try
        {
            var flight = await _flightService.UpdateFlight(userId, flightId, request);
            return Ok(flight);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating flight {FlightId} for user {UserId}", flightId, userId);
            return StatusCode(500, "An error occurred while updating the flight");
        }
    }

    /// <summary>
    /// Deletes a flight
    /// </summary>
    [HttpDelete("{userId}/{flightId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteFlight(string userId, string flightId)
    {
        try
        {
            await _flightService.DeleteFlight(userId, flightId);
            return Ok();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting flight {FlightId} for user {UserId}", flightId, userId);
            return StatusCode(500, "An error occurred while deleting the flight");
        }
    }

    /// <summary>
    /// Regenerates the navlog for a flight with updated weather data
    /// </summary>
    [HttpPost("[action]/{flightId}")]
    [ProducesResponseType(typeof(FlightDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<FlightDto>> RegenerateNavlog(string userId, string flightId)
    {
        try
        {
            var flight = await _flightService.RegenerateNavlog(userId, flightId);
            return Ok(flight);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error regenerating navlog for flight {FlightId} for user {UserId}", 
                flightId, userId);
            return StatusCode(500, "An error occurred while regenerating the navlog");
        }
    }
}