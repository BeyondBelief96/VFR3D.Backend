using Microsoft.AspNetCore.Mvc;
using VFR3D.API.Authentication;
using VFR3D.API.Models;
using VFR3D.Domain.Exceptions;
using VFR3D.Infrastructure.Dtos.Notam;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ConditionalAuth]
public class NotamController(INotamService notamService)
    : ControllerBase
{
    /// <summary>
    /// Gets NOTAMs for a specific airport
    /// </summary>
    /// <param name="icaoCodeOrIdent">ICAO code or FAA identifier (e.g., KDFW, DFW)</param>
    /// <returns>NOTAMs for the specified airport</returns>
    /// <response code="200">Returns the NOTAMs</response>
    /// <response code="400">If the identifier is invalid</response>
    [HttpGet("{icaoCodeOrIdent}")]
    [ProducesResponseType(typeof(NotamResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<NotamResponseDto>> GetNotamsForAirport(
        string icaoCodeOrIdent,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(icaoCodeOrIdent))
        {
            throw new ValidationException("icaoCodeOrIdent", "Airport identifier is required");
        }

        var result = await notamService.GetNotamsForAirportAsync(icaoCodeOrIdent, ct);
        return Ok(result);
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
    [HttpGet("radius")]
    [ProducesResponseType(typeof(NotamResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<NotamResponseDto>> GetNotamsByRadius(
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] double radiusNm,
        CancellationToken ct)
    {
        if (latitude < -90 || latitude > 90)
        {
            throw new ValidationException("latitude", "Latitude must be between -90 and 90 degrees");
        }

        if (longitude < -180 || longitude > 180)
        {
            throw new ValidationException("longitude", "Longitude must be between -180 and 180 degrees");
        }

        if (radiusNm <= 0 || radiusNm > 100)
        {
            throw new ValidationException("radiusNm", "Radius must be between 0 and 100 nautical miles");
        }

        var result = await notamService.GetNotamsByRadiusAsync(latitude, longitude, radiusNm, ct);
        return Ok(result);
    }

    /// <summary>
    /// Gets NOTAMs for a flight route (airports and/or waypoints)
    /// </summary>
    /// <param name="request">Route query request with airport identifiers and/or route points with coordinates</param>
    /// <returns>Aggregated and deduplicated NOTAMs for the route</returns>
    /// <remarks>
    /// The route can be specified using either:
    /// - AirportIdentifiers: Simple list of airport ICAO/FAA codes (e.g., ["KDFW", "KAUS"])
    /// - RoutePoints: Ordered list of mixed airports and waypoints with coordinates
    ///
    /// If both are provided, RoutePoints takes precedence.
    ///
    /// Example RoutePoints request:
    /// ```json
    /// {
    ///   "routePoints": [
    ///     { "airportIdentifier": "KDFW" },
    ///     { "name": "Lake Travis", "latitude": 30.4082, "longitude": -97.8538 },
    ///     { "latitude": 30.1, "longitude": -97.6, "radiusNm": 15 },
    ///     { "airportIdentifier": "KAUS" }
    ///   ],
    ///   "corridorRadiusNm": 25
    /// }
    /// ```
    /// </remarks>
    /// <response code="200">Returns the NOTAMs</response>
    /// <response code="400">If the request is invalid</response>
    [HttpPost("route")]
    [ProducesResponseType(typeof(NotamResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<NotamResponseDto>> GetNotamsForRoute(
        [FromBody] NotamQueryByRouteRequest request,
        CancellationToken ct)
    {
        if (request == null)
        {
            throw new ValidationException("request", "Request body is required");
        }

        var hasRoutePoints = request.RoutePoints is { Count: > 0 };
        var hasAirportIdentifiers = request.AirportIdentifiers is { Count: > 0 };

        if (!hasRoutePoints && !hasAirportIdentifiers)
        {
            throw new ValidationException("request", "At least one airport identifier or route point is required");
        }

        // Validate route points if provided
        if (hasRoutePoints)
        {
            ValidateRoutePoints(request.RoutePoints);
        }

        var result = await notamService.GetNotamsForRouteAsync(request, ct);
        return Ok(result);
    }

    private static void ValidateRoutePoints(List<RoutePointDto> routePoints)
    {
        for (var i = 0; i < routePoints.Count; i++)
        {
            var point = routePoints[i];
            var pointNumber = i + 1;

            var hasAirportId = !string.IsNullOrWhiteSpace(point.AirportIdentifier);
            var hasCoordinates = point.Latitude.HasValue && point.Longitude.HasValue;

            // Each point must be either an airport OR a waypoint with coordinates
            if (!hasAirportId && !hasCoordinates)
            {
                throw new ValidationException($"routePoints[{i}]",
                    $"Route point {pointNumber}: Must specify either an airport identifier or latitude/longitude coordinates");
            }

            // Validate coordinates if this is a waypoint
            if (!hasAirportId)
            {
                if (!point.Latitude.HasValue || !point.Longitude.HasValue)
                {
                    throw new ValidationException($"routePoints[{i}]",
                        $"Route point {pointNumber}: Waypoints require both latitude and longitude");
                }

                if (point.Latitude.Value < -90 || point.Latitude.Value > 90)
                {
                    throw new ValidationException($"routePoints[{i}].latitude",
                        $"Route point {pointNumber}: Latitude must be between -90 and 90 degrees");
                }

                if (point.Longitude.Value < -180 || point.Longitude.Value > 180)
                {
                    throw new ValidationException($"routePoints[{i}].longitude",
                        $"Route point {pointNumber}: Longitude must be between -180 and 180 degrees");
                }

                if (point.RadiusNm.HasValue && (point.RadiusNm.Value <= 0 || point.RadiusNm.Value > 100))
                {
                    throw new ValidationException($"routePoints[{i}].radiusNm",
                        $"Route point {pointNumber}: Radius must be between 0 and 100 nautical miles");
                }
            }
        }
    }
}
