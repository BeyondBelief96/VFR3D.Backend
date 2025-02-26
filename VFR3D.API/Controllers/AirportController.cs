using Amazon.SecretsManager.Model;
using Microsoft.AspNetCore.Mvc;
using VFR3D.API.Authentication;
using VFR3D.Infrastructure.Dtos;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ConditionalAuth]
    public class AirportController(
        IAirportService airportService,
        ILogger<AirportController> logger)
        : ControllerBase
    {
        /// <summary>
        /// Gets all airports, optionally filtered by search term
        /// </summary>
        /// <param name="search">Optional search term for ICAO ID or airport identifier</param>
        /// <returns>List of airports matching the search criteria</returns>
        /// <response code="200">Returns the list of airports</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AirportDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<AirportDto>>> GetAllAirports([FromQuery] string? search)
        {
            try
            {
                var airports = await airportService.GetAllAirports(search);
                return Ok(airports);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving airports with search: {Search}", search);
                return StatusCode(500, "An error occurred while retrieving airports");
            }
        }

        /// <summary>
        /// Gets an airport by its ICAO code or identifier
        /// </summary>
        /// <param name="icaoCodeOrIdent">ICAO code or airport identifier</param>
        /// <returns>Airport information</returns>
        /// <response code="200">Returns the airport information</response>
        /// <response code="404">If the airport is not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("{icaoCodeOrIdent}")]
        [ProducesResponseType(typeof(AirportDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AirportDto>> GetAirportByIcaoCodeOrIdent(string icaoCodeOrIdent)
        {
            try
            {
                var airport = await airportService.GetAirportByIcaoCodeOrIdent(icaoCodeOrIdent);
                return Ok(airport);
            }
            catch (ResourceNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving airport: {IcaoCodeOrIdent}", icaoCodeOrIdent);
                return StatusCode(500, "An error occurred while retrieving the airport");
            }
        }

        /// <summary>
        /// Gets all airports in a specific state
        /// </summary>
        /// <param name="stateCode">Two-letter state code</param>
        /// <returns>List of airports in the specified state</returns>
        /// <response code="200">Returns the list of airports</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("state/{stateCode}")]
        [ProducesResponseType(typeof(IEnumerable<AirportDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<AirportDto>>> GetAirportsByState(string stateCode)
        {
            try
            {
                var airports = await airportService.GetAirportsByState(stateCode);
                return Ok(airports);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving airports for state: {StateCode}", stateCode);
                return StatusCode(500, "An error occurred while retrieving airports");
            }
        }

        /// <summary>
        /// Gets all airports in multiple states
        /// </summary>
        /// <param name="stateCodes">Comma-separated list of two-letter state codes</param>
        /// <returns>List of airports in the specified states</returns>
        /// <response code="200">Returns the list of airports</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("states/{stateCodes}")]
        [ProducesResponseType(typeof(IEnumerable<AirportDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<AirportDto>>> GetAirportsByStates(string stateCodes)
        {
            try
            {
                var stateCodeArray = stateCodes.Split(',')
                    .Select(s => s.Trim())
                    .ToArray();

                var airports = await airportService.GetAirportsByStates(stateCodeArray);
                return Ok(airports);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving airports for states: {StateCodes}", stateCodes);
                return StatusCode(500, "An error occurred while retrieving airports");
            }
        }

        /// <summary>
        /// Gets airports by a batch of ICAO codes or identifiers
        /// </summary>
        /// <param name="icaoCodesOrIdents">Comma-separated list of ICAO codes or identifiers</param>
        /// <returns>List of airports matching the provided codes</returns>
        /// <response code="200">Returns the list of airports</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("batch/{icaoCodesOrIdents}")]
        [ProducesResponseType(typeof(IEnumerable<AirportDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<AirportDto>>> GetAirportsByIcaoCodesOrIdents(string icaoCodesOrIdents)
        {
            try
            {
                var codesArray = icaoCodesOrIdents.Split(',')
                    .Select(s => s.Trim())
                    .ToArray();

                var airports = await airportService.GetAirportsByIcaoCodesOrIdents(codesArray);
                return Ok(airports);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving airports for codes: {IcaoCodesOrIdents}", icaoCodesOrIdents);
                return StatusCode(500, "An error occurred while retrieving airports");
            }
        }
    }
}