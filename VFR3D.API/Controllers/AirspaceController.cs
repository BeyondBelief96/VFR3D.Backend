using Microsoft.AspNetCore.Mvc;
using VFR3D.API.Authentication;
using VFR3D.API.Models;
using VFR3D.Domain.Exceptions;
using VFR3D.Infrastructure.Dtos;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ConditionalAuth]
public class AirspaceController(IAirspaceService airspaceService)
    : ControllerBase
{
    /// <summary>
    /// Gets airspaces by their classes
    /// </summary>
    /// <param name="classes">Comma-separated list of airspace classes</param>
    /// <returns>List of airspaces matching the specified classes</returns>
    /// <response code="200">Returns the list of airspaces</response>
    /// <response code="400">If no classes are provided</response>
    [HttpGet("by-classes")]
    [ProducesResponseType(typeof(IEnumerable<AirspaceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<AirspaceDto>>> GetByClasses([FromQuery] string classes)
    {
        if (string.IsNullOrWhiteSpace(classes))
        {
            throw new ValidationException("classes", "Airspace classes are required");
        }

        var classArray = classes.Split(',')
            .Select(c => c.Trim())
            .ToArray();

        var airspaces = await airspaceService.GetByClasses(classArray);
        return Ok(airspaces);
    }

    /// <summary>
    /// Gets airspaces by cities
    /// </summary>
    /// <param name="cities">Comma-separated list of city names</param>
    /// <returns>List of airspaces for the specified cities</returns>
    /// <response code="200">Returns the list of airspaces</response>
    /// <response code="400">If no cities are provided</response>
    [HttpGet("by-cities")]
    [ProducesResponseType(typeof(IEnumerable<AirspaceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<AirspaceDto>>> GetByCity([FromQuery] string cities)
    {
        if (string.IsNullOrWhiteSpace(cities))
        {
            throw new ValidationException("cities", "Cities are required");
        }

        var cityArray = cities.Split(',')
            .Select(c => c.Trim())
            .ToArray();

        var airspaces = await airspaceService.GetByCities(cityArray);
        return Ok(airspaces);
    }

    /// <summary>
    /// Gets airspaces by states
    /// </summary>
    /// <param name="states">Comma-separated list of state codes</param>
    /// <returns>List of airspaces for the specified states</returns>
    /// <response code="200">Returns the list of airspaces</response>
    /// <response code="400">If no states are provided</response>
    [HttpGet("by-states")]
    [ProducesResponseType(typeof(IEnumerable<AirspaceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<AirspaceDto>>> GetByState([FromQuery] string states)
    {
        if (string.IsNullOrWhiteSpace(states))
        {
            throw new ValidationException("states", "States are required");
        }

        var stateArray = states.Split(',')
            .Select(s => s.Trim())
            .ToArray();

        var airspaces = await airspaceService.GetByStates(stateArray);
        return Ok(airspaces);
    }

    /// <summary>
    /// Gets special use airspaces by type codes
    /// </summary>
    /// <param name="typeCodes">Comma-separated list of type codes</param>
    /// <returns>List of special use airspaces matching the specified type codes</returns>
    /// <response code="200">Returns the list of special use airspaces</response>
    /// <response code="400">If no type codes are provided</response>
    [HttpGet("special-use/by-type-codes")]
    [ProducesResponseType(typeof(IEnumerable<SpecialUseAirspaceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<SpecialUseAirspaceDto>>> GetByTypeCode([FromQuery] string typeCodes)
    {
        if (string.IsNullOrWhiteSpace(typeCodes))
        {
            throw new ValidationException("typeCodes", "Type codes are required");
        }

        var typeCodeArray = typeCodes.Split(',')
            .Select(tc => tc.Trim())
            .ToArray();

        var airspaces = await airspaceService.GetByTypeCodes(typeCodeArray);
        return Ok(airspaces);
    }

    /// <summary>
    /// Gets airspaces by ICAO codes or identifiers
    /// </summary>
    /// <param name="icaoOrIdents">Comma-separated list of ICAO codes or identifiers</param>
    /// <returns>List of airspaces matching the specified ICAO codes or identifiers</returns>
    /// <response code="200">Returns the list of airspaces</response>
    /// <response code="400">If no ICAO codes or identifiers are provided</response>
    [HttpGet("by-icao-or-idents")]
    [ProducesResponseType(typeof(IEnumerable<AirspaceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<AirspaceDto>>> GetByIcaoOrIdent([FromQuery] string icaoOrIdents)
    {
        if (string.IsNullOrWhiteSpace(icaoOrIdents))
        {
            throw new ValidationException("icaoOrIdents", "ICAO codes or identifiers are required");
        }

        var idArray = icaoOrIdents.Split(',')
            .Select(id => id.Trim())
            .ToArray();

        var airspaces = await airspaceService.GetByIcaoOrIdents(idArray);
        return Ok(airspaces);
    }

    /// <summary>
    /// Gets airspaces by global IDs
    /// </summary>
    /// <param name="globalIds">Comma-separated list of global IDs</param>
    /// <returns>List of airspaces matching the specified global IDs</returns>
    /// <response code="200">Returns the list of airspaces</response>
    /// <response code="400">If no global IDs are provided</response>
    [HttpGet("by-global-ids")]
    [ProducesResponseType(typeof(IEnumerable<AirspaceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<AirspaceDto>>> GetByGlobalIds([FromQuery] string globalIds)
    {
        if (string.IsNullOrWhiteSpace(globalIds))
        {
            throw new ValidationException("globalIds", "Global IDs are required");
        }

        var idArray = globalIds.Split(',')
            .Select(id => id.Trim())
            .ToArray();

        var airspaces = await airspaceService.GetByGlobalIds(idArray);
        return Ok(airspaces);
    }

    /// <summary>
    /// Gets special use airspaces by global IDs
    /// </summary>
    /// <param name="globalIds">Comma-separated list of global IDs</param>
    /// <returns>List of special use airspaces matching the specified global IDs</returns>
    /// <response code="200">Returns the list of special use airspaces</response>
    /// <response code="400">If no global IDs are provided</response>
    [HttpGet("special-use/by-global-ids")]
    [ProducesResponseType(typeof(IEnumerable<SpecialUseAirspaceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<SpecialUseAirspaceDto>>> GetSpecialUseByGlobalIds([FromQuery] string globalIds)
    {
        if (string.IsNullOrWhiteSpace(globalIds))
        {
            throw new ValidationException("globalIds", "Global IDs are required");
        }

        var idArray = globalIds.Split(',')
            .Select(id => id.Trim())
            .ToArray();

        var airspaces = await airspaceService.GetSpecialUseByGlobalIds(idArray);
        return Ok(airspaces);
    }
}
