using Microsoft.AspNetCore.Mvc;
using VFR3D.API.Authentication;
using VFR3D.API.Models;
using VFR3D.Domain.Exceptions;
using VFR3D.Infrastructure.Dtos.Aircraft;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ConditionalAuth]
public class AircraftController(IAircraftService aircraftService)
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
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AircraftDto>> CreateAircraft(
        string userId,
        [FromBody] CreateAircraftRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.TailNumber))
        {
            throw new ValidationException("TailNumber", "Tail number is required");
        }

        if (string.IsNullOrWhiteSpace(request.AircraftType))
        {
            throw new ValidationException("AircraftType", "Aircraft type is required");
        }

        var aircraft = await aircraftService.CreateAircraft(userId, request);
        return Ok(aircraft);
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
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AircraftDto>> UpdateAircraft(
        string userId,
        string id,
        [FromBody] UpdateAircraftRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.TailNumber))
        {
            throw new ValidationException("TailNumber", "Tail number is required");
        }

        if (string.IsNullOrWhiteSpace(request.AircraftType))
        {
            throw new ValidationException("AircraftType", "Aircraft type is required");
        }

        var aircraft = await aircraftService.UpdateAircraft(userId, id, request);
        return Ok(aircraft);
    }

    /// <summary>
    /// Gets a single aircraft with its performance profiles
    /// </summary>
    /// <param name="userId">The ID of the user who owns the aircraft</param>
    /// <param name="id">The ID of the aircraft</param>
    /// <returns>The aircraft with its performance profiles</returns>
    [HttpGet("{userId}/{id}")]
    [ProducesResponseType(typeof(AircraftDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AircraftDto>> GetAircraft(string userId, string id)
    {
        var aircraft = await aircraftService.GetAircraft(userId, id);
        if (aircraft == null)
        {
            throw new AircraftNotFoundException(userId, id);
        }
        return Ok(aircraft);
    }

    /// <summary>
    /// Gets all aircraft for a user
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <returns>List of aircraft with their performance profiles</returns>
    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(List<AircraftDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AircraftDto>>> GetAircraftByUserId(string userId)
    {
        var aircraft = await aircraftService.GetAircraftByUserId(userId);
        return Ok(aircraft);
    }

    /// <summary>
    /// Deletes an aircraft
    /// </summary>
    /// <param name="userId">The ID of the user who owns the aircraft</param>
    /// <param name="id">The ID of the aircraft to delete</param>
    [HttpDelete("{userId}/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteAircraft(string userId, string id)
    {
        await aircraftService.DeleteAircraft(userId, id);
        return Ok();
    }
}
