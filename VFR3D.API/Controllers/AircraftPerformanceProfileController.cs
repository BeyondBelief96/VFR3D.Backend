using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VFR3D.API.Authentication;
using VFR3D.Infrastructure.Dtos.AircraftPerformanceProfiles;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ConditionalAuth]
public class AircraftPerformanceProfileController(
    IAircraftPerformanceProfileService performanceProfileService,
    ILogger<AircraftPerformanceProfileController> logger)
    : ControllerBase
{
    /// <summary>
    /// Creates a new aircraft performance profile
    /// </summary>
    /// <param name="request">The profile data to save</param>
    /// <returns>The created performance profile</returns>
    [HttpPost]
    [ProducesResponseType(typeof(AircraftPerformanceProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AircraftPerformanceProfileDto>> SaveProfile(
        [FromBody] SaveAircraftPerformanceProfileRequestDto request)
    {
        try
        {
            var profile = await performanceProfileService.SaveProfile(request);
            return Ok(profile);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving aircraft performance profile");
            return StatusCode(500, "An error occurred while saving the performance profile");
        }
    }

    /// <summary>
    /// Updates an existing aircraft performance profile
    /// </summary>
    /// <param name="id">The ID of the profile to update</param>
    /// <param name="request">The updated profile data</param>
    /// <returns>The updated performance profile</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(AircraftPerformanceProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AircraftPerformanceProfileDto>> UpdateProfile(
        string id,
        [FromBody] UpdateAircraftPerformanceProfileRequestDto request)
    {
        try
        {
            var profile = await performanceProfileService.UpdateProfile(id, request);
            return Ok(profile);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating aircraft performance profile {ProfileId}", id);
            return StatusCode(500, "An error occurred while updating the performance profile");
        }
    }

    /// <summary>
    /// Gets all performance profiles for a user
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <returns>List of performance profiles</returns>
    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(List<AircraftPerformanceProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<AircraftPerformanceProfileDto>>> GetProfilesByUserId(string userId)
    {
        try
        {
            var profiles = await performanceProfileService.GetProfilesByUserId(userId);
            return Ok(profiles);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting aircraft performance profiles for user {UserId}", userId);
            return StatusCode(500, "An error occurred while retrieving the performance profiles");
        }
    }

    /// <summary>
    /// Deletes a performance profile
    /// </summary>
    /// <param name="userId">The ID of the user who owns the profile</param>
    /// <param name="id">The ID of the profile to delete</param>
    [HttpDelete("{userId}/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteProfile(string userId, string id)
    {
        try
        {
            await performanceProfileService.DeleteProfile(userId, id);
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
            logger.LogError(ex, "Error deleting aircraft performance profile {ProfileId} for user {UserId}", 
                id, userId);
            return StatusCode(500, "An error occurred while deleting the performance profile");
        }
    }
}