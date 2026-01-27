using Microsoft.AspNetCore.Mvc;
using VFR3D.API.Authentication;
using VFR3D.Infrastructure.Dtos.Aircraft;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ConditionalAuth]
public class AircraftController(
    IAircraftService aircraftService,
    ILogger<AircraftController> logger)
    : ControllerBase
{
    /// <summary>
    /// Creates a new aircraft for a user
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="request">The aircraft data to create</param>
    /// <returns>The created aircraft</returns>
    [HttpPost("{userId}")]
    [ProducesResponseType(typeof(AircraftDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AircraftDto>> CreateAircraft(
        string userId,
        [FromBody] CreateAircraftRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.TailNumber))
        {
            return BadRequest("Tail number is required");
        }

        if (string.IsNullOrWhiteSpace(request.AircraftType))
        {
            return BadRequest("Aircraft type is required");
        }

        try
        {
            var aircraft = await aircraftService.CreateAircraft(userId, request);
            return Ok(aircraft);
        }
        catch (System.Data.DuplicateNameException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating aircraft for user {UserId}", userId);
            return StatusCode(500, "An error occurred while creating the aircraft");
        }
    }

    /// <summary>
    /// Updates an existing aircraft
    /// </summary>
    /// <param name="userId">The ID of the user who owns the aircraft</param>
    /// <param name="id">The ID of the aircraft to update</param>
    /// <param name="request">The updated aircraft data</param>
    /// <returns>The updated aircraft</returns>
    [HttpPut("{userId}/{id}")]
    [ProducesResponseType(typeof(AircraftDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AircraftDto>> UpdateAircraft(
        string userId,
        string id,
        [FromBody] UpdateAircraftRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.TailNumber))
        {
            return BadRequest("Tail number is required");
        }

        if (string.IsNullOrWhiteSpace(request.AircraftType))
        {
            return BadRequest("Aircraft type is required");
        }

        try
        {
            var aircraft = await aircraftService.UpdateAircraft(userId, id, request);
            return Ok(aircraft);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (System.Data.DuplicateNameException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating aircraft {AircraftId} for user {UserId}", id, userId);
            return StatusCode(500, "An error occurred while updating the aircraft");
        }
    }

    /// <summary>
    /// Gets a single aircraft with its performance profiles
    /// </summary>
    /// <param name="userId">The ID of the user who owns the aircraft</param>
    /// <param name="id">The ID of the aircraft</param>
    /// <returns>The aircraft with its performance profiles</returns>
    [HttpGet("{userId}/{id}")]
    [ProducesResponseType(typeof(AircraftDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AircraftDto>> GetAircraft(string userId, string id)
    {
        try
        {
            var aircraft = await aircraftService.GetAircraft(userId, id);
            if (aircraft == null)
            {
                return NotFound($"Aircraft not found with ID {id}");
            }
            return Ok(aircraft);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting aircraft {AircraftId} for user {UserId}", id, userId);
            return StatusCode(500, "An error occurred while retrieving the aircraft");
        }
    }

    /// <summary>
    /// Gets all aircraft for a user
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <returns>List of aircraft with their performance profiles</returns>
    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(List<AircraftDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<AircraftDto>>> GetAircraftByUserId(string userId)
    {
        try
        {
            var aircraft = await aircraftService.GetAircraftByUserId(userId);
            return Ok(aircraft);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting aircraft for user {UserId}", userId);
            return StatusCode(500, "An error occurred while retrieving the aircraft");
        }
    }

    /// <summary>
    /// Deletes an aircraft
    /// </summary>
    /// <param name="userId">The ID of the user who owns the aircraft</param>
    /// <param name="id">The ID of the aircraft to delete</param>
    [HttpDelete("{userId}/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteAircraft(string userId, string id)
    {
        try
        {
            await aircraftService.DeleteAircraft(userId, id);
            return Ok();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting aircraft {AircraftId} for user {UserId}", id, userId);
            return StatusCode(500, "An error occurred while deleting the aircraft");
        }
    }
}
