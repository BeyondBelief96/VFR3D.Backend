using Microsoft.AspNetCore.Mvc;
using VFR3D.API.Authentication;
using VFR3D.Infrastructure.Dtos;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ConditionalAuth]
    public class AirspaceController(
        IAirspaceService airspaceService,
        ILogger<AirspaceController> logger)
        : ControllerBase
    {
        /// <summary>
        /// Gets airspaces by their classes
        /// </summary>
        /// <param name="classes">Comma-separated list of airspace classes</param>
        /// <returns>List of airspaces matching the specified classes</returns>
        /// <response code="200">Returns the list of airspaces</response>
        /// <response code="400">If no classes are provided</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("by-classes")]
        [ProducesResponseType(typeof(IEnumerable<AirspaceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<AirspaceDto>>> GetByClasses([FromQuery] string classes)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(classes))
                {
                    return BadRequest("Airspace classes are required");
                }

                var classArray = classes.Split(',')
                    .Select(c => c.Trim())
                    .ToArray();

                var airspaces = await airspaceService.GetByClasses(classArray);
                return Ok(airspaces);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving airspaces by classes: {Classes}", classes);
                return StatusCode(500, "An error occurred while retrieving airspaces");
            }
        }

        /// <summary>
        /// Gets airspaces by cities
        /// </summary>
        /// <param name="cities">Comma-separated list of city names</param>
        /// <returns>List of airspaces for the specified cities</returns>
        /// <response code="200">Returns the list of airspaces</response>
        /// <response code="400">If no cities are provided</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("by-cities")]
        [ProducesResponseType(typeof(IEnumerable<AirspaceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<AirspaceDto>>> GetByCity([FromQuery] string cities)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cities))
                {
                    return BadRequest("Cities are required");
                }

                var cityArray = cities.Split(',')
                    .Select(c => c.Trim())
                    .ToArray();

                var airspaces = await airspaceService.GetByCities(cityArray);
                return Ok(airspaces);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving airspaces by cities: {Cities}", cities);
                return StatusCode(500, "An error occurred while retrieving airspaces");
            }
        }

        /// <summary>
        /// Gets airspaces by states
        /// </summary>
        /// <param name="states">Comma-separated list of state codes</param>
        /// <returns>List of airspaces for the specified states</returns>
        /// <response code="200">Returns the list of airspaces</response>
        /// <response code="400">If no states are provided</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("by-states")]
        [ProducesResponseType(typeof(IEnumerable<AirspaceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<AirspaceDto>>> GetByState([FromQuery] string states)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(states))
                {
                    return BadRequest("States are required");
                }

                var stateArray = states.Split(',')
                    .Select(s => s.Trim())
                    .ToArray();

                var airspaces = await airspaceService.GetByStates(stateArray);
                return Ok(airspaces);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving airspaces by states: {States}", states);
                return StatusCode(500, "An error occurred while retrieving airspaces");
            }
        }

        /// <summary>
        /// Gets special use airspaces by type codes
        /// </summary>
        /// <param name="typeCodes">Comma-separated list of type codes</param>
        /// <returns>List of special use airspaces matching the specified type codes</returns>
        /// <response code="200">Returns the list of special use airspaces</response>
        /// <response code="400">If no type codes are provided</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("special-use/by-type-codes")]
        [ProducesResponseType(typeof(IEnumerable<SpecialUseAirspaceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<SpecialUseAirspaceDto>>> GetByTypeCode([FromQuery] string typeCodes)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(typeCodes))
                {
                    return BadRequest("Type codes are required");
                }

                var typeCodeArray = typeCodes.Split(',')
                    .Select(tc => tc.Trim())
                    .ToArray();

                var airspaces = await airspaceService.GetByTypeCodes(typeCodeArray);
                return Ok(airspaces);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving special use airspaces by type codes: {TypeCodes}", typeCodes);
                return StatusCode(500, "An error occurred while retrieving special use airspaces");
            }
        }

        /// <summary>
        /// Gets airspaces by ICAO codes or identifiers
        /// </summary>
        /// <param name="icaoOrIdents">Comma-separated list of ICAO codes or identifiers</param>
        /// <returns>List of airspaces matching the specified ICAO codes or identifiers</returns>
        /// <response code="200">Returns the list of airspaces</response>
        /// <response code="400">If no ICAO codes or identifiers are provided</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("by-icao-or-idents")]
        [ProducesResponseType(typeof(IEnumerable<AirspaceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<AirspaceDto>>> GetByIcaoOrIdent([FromQuery] string icaoOrIdents)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(icaoOrIdents))
                {
                    return BadRequest("ICAO codes or identifiers are required");
                }

                var idArray = icaoOrIdents.Split(',')
                    .Select(id => id.Trim())
                    .ToArray();

                var airspaces = await airspaceService.GetByIcaoOrIdents(idArray);
                return Ok(airspaces);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving airspaces by ICAO codes or identifiers: {IcaoOrIdents}", 
                    icaoOrIdents);
                return StatusCode(500, "An error occurred while retrieving airspaces");
            }
        }

        /// <summary>
        /// Gets airspaces by global IDs
        /// </summary>
        /// <param name="globalIds">Comma-separated list of global IDs</param>
        /// <returns>List of airspaces matching the specified global IDs</returns>
        /// <response code="200">Returns the list of airspaces</response>
        /// <response code="400">If no global IDs are provided</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("by-global-ids")]
        [ProducesResponseType(typeof(IEnumerable<AirspaceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<AirspaceDto>>> GetByGlobalIds([FromQuery] string globalIds)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(globalIds))
                {
                    return BadRequest("Global IDs are required");
                }

                var idArray = globalIds.Split(',')
                    .Select(id => id.Trim())
                    .ToArray();

                var airspaces = await airspaceService.GetByGlobalIds(idArray);
                return Ok(airspaces);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving airspaces by global IDs: {GlobalIds}", globalIds);
                return StatusCode(500, "An error occurred while retrieving airspaces by global IDs");
            }
        }

        /// <summary>
        /// Gets special use airspaces by global IDs
        /// </summary>
        /// <param name="globalIds">Comma-separated list of global IDs</param>
        /// <returns>List of special use airspaces matching the specified global IDs</returns>
        /// <response code="200">Returns the list of special use airspaces</response>
        /// <response code="400">If no global IDs are provided</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("special-use/by-global-ids")]
        [ProducesResponseType(typeof(IEnumerable<SpecialUseAirspaceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<SpecialUseAirspaceDto>>> GetSpecialUseByGlobalIds([FromQuery] string globalIds)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(globalIds))
                {
                    return BadRequest("Global IDs are required");
                }

                var idArray = globalIds.Split(',')
                    .Select(id => id.Trim())
                    .ToArray();

                var airspaces = await airspaceService.GetSpecialUseByGlobalIds(idArray);
                return Ok(airspaces);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving special use airspaces by global IDs: {GlobalIds}", globalIds);
                return StatusCode(500, "An error occurred while retrieving special use airspaces by global IDs");
            }
        }
    }
}