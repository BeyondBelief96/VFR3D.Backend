using Microsoft.AspNetCore.Mvc;
using VFR3D.API.Authentication;
using VFR3D.API.Models;
using VFR3D.Domain.Exceptions;
using VFR3D.Infrastructure.Dtos.AircraftPerformanceProfiles;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ConditionalAuth]
public class AircraftPerformanceProfileController(
    IAircraftPerformanceProfileService performanceProfileService)
    : ControllerBase
{
    /// <summary>
    /// Creates a new aircraft performance profile
    /// </summary>
    /// <param name="request">The profile data to save</param>
    /// <returns>The created performance profile</returns>
    [HttpPost]
    [ProducesResponseType(typeof(AircraftPerformanceProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AircraftPerformanceProfileDto>> SaveProfile(
        [FromBody] SaveAircraftPerformanceProfileRequestDto request)
    {
        var profile = await performanceProfileService.SaveProfile(request);
        return Ok(profile);
    }

    /// <summary>
    /// Updates an existing aircraft performance profile
    /// </summary>
    /// <param name="id">The ID of the profile to update</param>
    /// <param name="request">The updated profile data</param>
    /// <returns>The updated performance profile</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(AircraftPerformanceProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AircraftPerformanceProfileDto>> UpdateProfile(
        string id,
        [FromBody] UpdateAircraftPerformanceProfileRequestDto request)
    {
        var profile = await performanceProfileService.UpdateProfile(id, request);
        return Ok(profile);
    }

    /// <summary>
    /// Gets all performance profiles for a user
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <returns>List of performance profiles</returns>
    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(List<AircraftPerformanceProfileDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AircraftPerformanceProfileDto>>> GetProfilesByUserId(string userId)
    {
        var profiles = await performanceProfileService.GetProfilesByUserId(userId);
        return Ok(profiles);
    }

    /// <summary>
    /// Deletes a performance profile
    /// </summary>
    /// <param name="userId">The ID of the user who owns the profile</param>
    /// <param name="id">The ID of the profile to delete</param>
    [HttpDelete("{userId}/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteProfile(string userId, string id)
    {
        await performanceProfileService.DeleteProfile(userId, id);
        return Ok();
    }
}
