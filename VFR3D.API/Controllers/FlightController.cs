using Microsoft.AspNetCore.Mvc;
using VFR3D.API.Authentication;
using VFR3D.API.Models;
using VFR3D.Domain.Exceptions;
using VFR3D.Infrastructure.Dtos.Flights;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ConditionalAuth]
public class FlightController(IFlightService flightService)
    : ControllerBase
{
    /// <summary>
    /// Gets all flights for a user
    /// </summary>
    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(List<FlightDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<FlightDto>>> GetFlights(string userId)
    {
        var flights = await flightService.GetFlights(userId);
        return Ok(flights);
    }

    /// <summary>
    /// Gets a specific flight by ID
    /// </summary>
    [HttpGet("{userId}/{flightId}")]
    [ProducesResponseType(typeof(FlightDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FlightDto>> GetFlight(string userId, string flightId)
    {
        var flight = await flightService.GetFlight(userId, flightId);
        return Ok(flight);
    }

    /// <summary>
    /// Creates a new flight
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(FlightDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FlightDto>> CreateFlight(
        string userId,
        [FromBody] CreateFlightRequestDto request)
    {
        var flight = await flightService.CreateFlight(userId, request);
        return Ok(flight);
    }

    /// <summary>
    /// Creates a round trip flight (outbound and return)
    /// </summary>
    [HttpPost("roundtrip")]
    [ProducesResponseType(typeof((FlightDto Outbound, FlightDto Return)), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<(FlightDto Outbound, FlightDto Return)>> CreateRoundTripFlight(
        string userId,
        [FromBody] CreateRoundTripFlightRequestDto request)
    {
        var (outbound, @return) = await flightService.CreateRoundTripFlight(userId, request);
        return Ok(new { Outbound = outbound, Return = @return });
    }

    /// <summary>
    /// Updates an existing flight
    /// </summary>
    [HttpPatch("{userId}/{flightId}")]
    [ProducesResponseType(typeof(FlightDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FlightDto>> UpdateFlight(
        string userId,
        string flightId,
        [FromBody] UpdateFlightRequestDto request)
    {
        var flight = await flightService.UpdateFlight(userId, flightId, request);
        return Ok(flight);
    }

    /// <summary>
    /// Deletes a flight
    /// </summary>
    [HttpDelete("{userId}/{flightId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFlight(string userId, string flightId)
    {
        await flightService.DeleteFlight(userId, flightId);
        return Ok();
    }

    /// <summary>
    /// Regenerates the navlog for a flight with updated weather data
    /// </summary>
    [HttpPost("[action]/{flightId}")]
    [ProducesResponseType(typeof(FlightDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FlightDto>> RegenerateNavlog(string userId, string flightId)
    {
        var flight = await flightService.RegenerateNavlog(userId, flightId);
        return Ok(flight);
    }
}
