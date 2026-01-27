using Microsoft.AspNetCore.Mvc;
using VFR3D.API.Authentication;
using VFR3D.Infrastructure.Dtos.WeightBalance;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ConditionalAuth]
public class WeightBalanceController(
    IWeightBalanceProfileService weightBalanceService,
    ILogger<WeightBalanceController> logger)
    : ControllerBase
{
    /// <summary>
    /// Creates a new Weight & Balance profile for a user
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="request">The W&B profile data to create</param>
    /// <returns>The created W&B profile</returns>
    [HttpPost("{userId}")]
    [ProducesResponseType(typeof(WeightBalanceProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<WeightBalanceProfileDto>> CreateProfile(
        string userId,
        [FromBody] CreateWeightBalanceProfileRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.ProfileName))
        {
            return BadRequest("Profile name is required");
        }

        if (request.EmptyWeight <= 0)
        {
            return BadRequest("Empty weight must be greater than zero");
        }

        if (request.MaxTakeoffWeight <= 0)
        {
            return BadRequest("Max takeoff weight must be greater than zero");
        }

        try
        {
            var profile = await weightBalanceService.CreateProfile(userId, request);
            return Ok(profile);
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
            logger.LogError(ex, "Error creating W&B profile for user {UserId}", userId);
            return StatusCode(500, "An error occurred while creating the W&B profile");
        }
    }

    /// <summary>
    /// Gets all Weight & Balance profiles for a user
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <returns>List of W&B profiles</returns>
    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(List<WeightBalanceProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<WeightBalanceProfileDto>>> GetProfilesByUser(string userId)
    {
        try
        {
            var profiles = await weightBalanceService.GetProfilesByUser(userId);
            return Ok(profiles);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting W&B profiles for user {UserId}", userId);
            return StatusCode(500, "An error occurred while retrieving W&B profiles");
        }
    }

    /// <summary>
    /// Gets a single Weight & Balance profile
    /// </summary>
    /// <param name="userId">The ID of the user who owns the profile</param>
    /// <param name="profileId">The ID of the profile</param>
    /// <returns>The W&B profile</returns>
    [HttpGet("{userId}/{profileId}")]
    [ProducesResponseType(typeof(WeightBalanceProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<WeightBalanceProfileDto>> GetProfile(string userId, string profileId)
    {
        try
        {
            var profile = await weightBalanceService.GetProfile(userId, profileId);
            if (profile == null)
            {
                return NotFound($"W&B profile not found with ID {profileId}");
            }
            return Ok(profile);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting W&B profile {ProfileId} for user {UserId}", profileId, userId);
            return StatusCode(500, "An error occurred while retrieving the W&B profile");
        }
    }

    /// <summary>
    /// Gets all Weight & Balance profiles for a specific aircraft
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="aircraftId">The ID of the aircraft</param>
    /// <returns>List of W&B profiles for the aircraft</returns>
    [HttpGet("{userId}/aircraft/{aircraftId}")]
    [ProducesResponseType(typeof(List<WeightBalanceProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<WeightBalanceProfileDto>>> GetProfilesByAircraft(string userId, string aircraftId)
    {
        try
        {
            var profiles = await weightBalanceService.GetProfilesByAircraft(userId, aircraftId);
            return Ok(profiles);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting W&B profiles for aircraft {AircraftId} user {UserId}", aircraftId, userId);
            return StatusCode(500, "An error occurred while retrieving W&B profiles");
        }
    }

    /// <summary>
    /// Updates an existing Weight & Balance profile
    /// </summary>
    /// <param name="userId">The ID of the user who owns the profile</param>
    /// <param name="profileId">The ID of the profile to update</param>
    /// <param name="request">The updated profile data</param>
    /// <returns>The updated W&B profile</returns>
    [HttpPut("{userId}/{profileId}")]
    [ProducesResponseType(typeof(WeightBalanceProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<WeightBalanceProfileDto>> UpdateProfile(
        string userId,
        string profileId,
        [FromBody] UpdateWeightBalanceProfileRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.ProfileName))
        {
            return BadRequest("Profile name is required");
        }

        if (request.EmptyWeight <= 0)
        {
            return BadRequest("Empty weight must be greater than zero");
        }

        if (request.MaxTakeoffWeight <= 0)
        {
            return BadRequest("Max takeoff weight must be greater than zero");
        }

        try
        {
            var profile = await weightBalanceService.UpdateProfile(userId, profileId, request);
            return Ok(profile);
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
            logger.LogError(ex, "Error updating W&B profile {ProfileId} for user {UserId}", profileId, userId);
            return StatusCode(500, "An error occurred while updating the W&B profile");
        }
    }

    /// <summary>
    /// Deletes a Weight & Balance profile
    /// </summary>
    /// <param name="userId">The ID of the user who owns the profile</param>
    /// <param name="profileId">The ID of the profile to delete</param>
    [HttpDelete("{userId}/{profileId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteProfile(string userId, string profileId)
    {
        try
        {
            await weightBalanceService.DeleteProfile(userId, profileId);
            return Ok();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting W&B profile {ProfileId} for user {UserId}", profileId, userId);
            return StatusCode(500, "An error occurred while deleting the W&B profile");
        }
    }

    /// <summary>
    /// Performs a Weight & Balance calculation
    /// </summary>
    /// <param name="userId">The ID of the user who owns the profile</param>
    /// <param name="profileId">The ID of the profile to use for calculation</param>
    /// <param name="request">The calculation request with loaded stations and optional fuel burn</param>
    /// <returns>The calculation result with takeoff/landing CG, station breakdown, and warnings</returns>
    [HttpPost("{userId}/{profileId}/calculate")]
    [ProducesResponseType(typeof(WeightBalanceCalculationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<WeightBalanceCalculationResultDto>> Calculate(
        string userId,
        string profileId,
        [FromBody] WeightBalanceCalculationRequestDto request)
    {
        try
        {
            var result = await weightBalanceService.Calculate(userId, profileId, request);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calculating W&B for profile {ProfileId} user {UserId}", profileId, userId);
            return StatusCode(500, "An error occurred while performing the W&B calculation");
        }
    }
}
