using Microsoft.AspNetCore.Mvc;
using VFR3D.API.Authentication;
using VFR3D.API.Models;
using VFR3D.Domain.Exceptions;
using VFR3D.Infrastructure.Dtos.WeightBalance;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ConditionalAuth]
public class WeightBalanceController(IWeightBalanceProfileService weightBalanceService)
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
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WeightBalanceProfileDto>> CreateProfile(
        string userId,
        [FromBody] CreateWeightBalanceProfileRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.ProfileName))
            throw new ValidationException("ProfileName", "Profile name is required");

        if (request.EmptyWeight <= 0)
            throw new ValidationException("EmptyWeight", "Empty weight must be greater than zero");

        if (request.MaxTakeoffWeight <= 0)
            throw new ValidationException("MaxTakeoffWeight", "Max takeoff weight must be greater than zero");

        var profile = await weightBalanceService.CreateProfile(userId, request);
        return Ok(profile);
    }

    /// <summary>
    /// Gets all Weight & Balance profiles for a user
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <returns>List of W&B profiles</returns>
    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(List<WeightBalanceProfileDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<WeightBalanceProfileDto>>> GetProfilesByUser(string userId)
    {
        var profiles = await weightBalanceService.GetProfilesByUser(userId);
        return Ok(profiles);
    }

    /// <summary>
    /// Gets a single Weight & Balance profile
    /// </summary>
    /// <param name="userId">The ID of the user who owns the profile</param>
    /// <param name="profileId">The ID of the profile</param>
    /// <returns>The W&B profile</returns>
    [HttpGet("{userId}/{profileId:guid}")]
    [ProducesResponseType(typeof(WeightBalanceProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WeightBalanceProfileDto>> GetProfile(string userId, Guid profileId)
    {
        var profile = await weightBalanceService.GetProfile(userId, profileId);
        if (profile == null)
            throw new WeightBalanceProfileNotFoundException(profileId.ToString());
        return Ok(profile);
    }

    /// <summary>
    /// Gets all Weight & Balance profiles for a specific aircraft
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="aircraftId">The ID of the aircraft</param>
    /// <returns>List of W&B profiles for the aircraft</returns>
    [HttpGet("{userId}/aircraft/{aircraftId}")]
    [ProducesResponseType(typeof(List<WeightBalanceProfileDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<WeightBalanceProfileDto>>> GetProfilesByAircraft(string userId, string aircraftId)
    {
        var profiles = await weightBalanceService.GetProfilesByAircraft(userId, aircraftId);
        return Ok(profiles);
    }

    /// <summary>
    /// Updates an existing Weight & Balance profile
    /// </summary>
    /// <param name="userId">The ID of the user who owns the profile</param>
    /// <param name="profileId">The ID of the profile to update</param>
    /// <param name="request">The updated profile data</param>
    /// <returns>The updated W&B profile</returns>
    [HttpPut("{userId}/{profileId:guid}")]
    [ProducesResponseType(typeof(WeightBalanceProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WeightBalanceProfileDto>> UpdateProfile(
        string userId,
        Guid profileId,
        [FromBody] UpdateWeightBalanceProfileRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.ProfileName))
            throw new ValidationException("ProfileName", "Profile name is required");

        if (request.EmptyWeight <= 0)
            throw new ValidationException("EmptyWeight", "Empty weight must be greater than zero");

        if (request.MaxTakeoffWeight <= 0)
            throw new ValidationException("MaxTakeoffWeight", "Max takeoff weight must be greater than zero");

        var profile = await weightBalanceService.UpdateProfile(userId, profileId, request);
        return Ok(profile);
    }

    /// <summary>
    /// Deletes a Weight & Balance profile
    /// </summary>
    /// <param name="userId">The ID of the user who owns the profile</param>
    /// <param name="profileId">The ID of the profile to delete</param>
    [HttpDelete("{userId}/{profileId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProfile(string userId, Guid profileId)
    {
        await weightBalanceService.DeleteProfile(userId, profileId);
        return Ok();
    }

    /// <summary>
    /// Performs a Weight & Balance calculation
    /// </summary>
    /// <param name="userId">The ID of the user who owns the profile</param>
    /// <param name="profileId">The ID of the profile to use for calculation</param>
    /// <param name="request">The calculation request with loaded stations and optional fuel burn</param>
    /// <returns>The calculation result with takeoff/landing CG, station breakdown, and warnings</returns>
    [HttpPost("{userId}/{profileId:guid}/calculate")]
    [ProducesResponseType(typeof(WeightBalanceCalculationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WeightBalanceCalculationResultDto>> Calculate(
        string userId,
        Guid profileId,
        [FromBody] WeightBalanceCalculationRequestDto request)
    {
        var result = await weightBalanceService.Calculate(userId, profileId, request);
        return Ok(result);
    }

    /// <summary>
    /// Calculates and persists a Weight & Balance calculation.
    /// If FlightId is provided, associates with that flight (one calculation per flight).
    /// If no FlightId, this is a standalone calculation for form repopulation (one per user).
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="request">The calculation request including profile ID and station loads</param>
    /// <returns>The saved calculation with full results</returns>
    [HttpPost("{userId}/calculate-and-save")]
    [ProducesResponseType(typeof(WeightBalanceCalculationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WeightBalanceCalculationDto>> CalculateAndSave(
        string userId,
        [FromBody] SaveWeightBalanceCalculationRequestDto request)
    {
        if (request.ProfileId == Guid.Empty)
            throw new ValidationException("ProfileId", "Profile ID is required");

        var result = await weightBalanceService.CalculateAndSave(userId, request);
        return Ok(result);
    }

    /// <summary>
    /// Gets a specific Weight & Balance calculation by ID.
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="calculationId">The ID of the calculation</param>
    /// <returns>The calculation with full results</returns>
    [HttpGet("{userId}/calculations/{calculationId:guid}")]
    [ProducesResponseType(typeof(WeightBalanceCalculationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WeightBalanceCalculationDto>> GetCalculation(string userId, Guid calculationId)
    {
        var calculation = await weightBalanceService.GetCalculation(userId, calculationId);
        if (calculation == null)
            throw new NotFoundException("WeightBalanceCalculation", calculationId);
        return Ok(calculation);
    }

    /// <summary>
    /// Gets the Weight & Balance calculation associated with a flight.
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="flightId">The ID of the flight</param>
    /// <returns>The calculation with full results, or 404 if no calculation exists for this flight</returns>
    [HttpGet("{userId}/flights/{flightId}/calculation")]
    [ProducesResponseType(typeof(WeightBalanceCalculationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WeightBalanceCalculationDto>> GetCalculationForFlight(string userId, string flightId)
    {
        var calculation = await weightBalanceService.GetCalculationForFlight(userId, flightId);
        if (calculation == null)
            throw new NotFoundException("WeightBalanceCalculation", $"flight:{flightId}");
        return Ok(calculation);
    }

    /// <summary>
    /// Gets the user's latest standalone calculation state for form repopulation.
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <returns>The standalone state with input values, or 404 if no standalone calculation exists</returns>
    [HttpGet("{userId}/standalone-state")]
    [ProducesResponseType(typeof(StandaloneCalculationStateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StandaloneCalculationStateDto>> GetLatestStandaloneState(string userId)
    {
        var state = await weightBalanceService.GetLatestStandaloneState(userId);
        if (state == null)
            throw new NotFoundException("StandaloneCalculationState", userId);
        return Ok(state);
    }

    /// <summary>
    /// Deletes a Weight & Balance calculation.
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="calculationId">The ID of the calculation to delete</param>
    [HttpDelete("{userId}/calculations/{calculationId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCalculation(string userId, Guid calculationId)
    {
        await weightBalanceService.DeleteCalculation(userId, calculationId);
        return Ok();
    }
}
