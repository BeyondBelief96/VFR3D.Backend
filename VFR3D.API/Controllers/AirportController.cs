using Microsoft.AspNetCore.Mvc;
using VFR3D.API.Authentication;
using VFR3D.API.Models;
using VFR3D.Infrastructure.Dtos;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ConditionalAuth]
    public class AirportController(
        IAirportService airportService,
        IRunwayService runwayService)
        : ControllerBase
    {
        /// <summary>
        /// Gets all airports, optionally filtered by search term
        /// </summary>
        /// <param name="search">Optional search term for ICAO ID or airport identifier</param>
        /// <returns>List of airports matching the search criteria</returns>
        /// <response code="200">Returns the list of airports</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AirportDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AirportDto>>> GetAllAirports([FromQuery] string? search)
        {
            var airports = await airportService.GetAllAirports(search);
            return Ok(airports);
        }

        /// <summary>
        /// Searches airports by ICAO code, FAA ID, name, or city
        /// </summary>
        /// <param name="query">Search query (minimum 2 characters)</param>
        /// <returns>List of airports matching the search criteria, prioritizing ICAO/ID matches</returns>
        /// <response code="200">Returns the list of matching airports (max 50)</response>
        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<AirportDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AirportDto>>> SearchAirports([FromQuery] string query)
        {
            var airports = await airportService.SearchAirports(query);
            return Ok(airports);
        }

        /// <summary>
        /// Gets an airport by its ICAO code or identifier
        /// </summary>
        /// <param name="icaoCodeOrIdent">ICAO code or airport identifier</param>
        /// <returns>Airport information</returns>
        /// <response code="200">Returns the airport information</response>
        /// <response code="404">If the airport is not found</response>
        [HttpGet("{icaoCodeOrIdent}")]
        [ProducesResponseType(typeof(AirportDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AirportDto>> GetAirportByIcaoCodeOrIdent(string icaoCodeOrIdent)
        {
            var airport = await airportService.GetAirportByIcaoCodeOrIdent(icaoCodeOrIdent);
            return Ok(airport);
        }

        /// <summary>
        /// Gets all airports in a specific state
        /// </summary>
        /// <param name="stateCode">Two-letter state code</param>
        /// <returns>List of airports in the specified state</returns>
        /// <response code="200">Returns the list of airports</response>
        [HttpGet("state/{stateCode}")]
        [ProducesResponseType(typeof(IEnumerable<AirportDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AirportDto>>> GetAirportsByState(string stateCode)
        {
            var airports = await airportService.GetAirportsByState(stateCode);
            return Ok(airports);
        }

        /// <summary>
        /// Gets all airports in multiple states
        /// </summary>
        /// <param name="stateCodes">Comma-separated list of two-letter state codes</param>
        /// <returns>List of airports in the specified states</returns>
        /// <response code="200">Returns the list of airports</response>
        [HttpGet("states/{stateCodes}")]
        [ProducesResponseType(typeof(IEnumerable<AirportDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AirportDto>>> GetAirportsByStates(string stateCodes)
        {
            var stateCodeArray = stateCodes.Split(',')
                .Select(s => s.Trim())
                .ToArray();

            var airports = await airportService.GetAirportsByStates(stateCodeArray);
            return Ok(airports);
        }

        /// <summary>
        /// Gets airports by a batch of ICAO codes or identifiers
        /// </summary>
        /// <param name="icaoCodesOrIdents">Comma-separated list of ICAO codes or identifiers</param>
        /// <returns>List of airports matching the provided codes</returns>
        /// <response code="200">Returns the list of airports</response>
        [HttpGet("batch/{icaoCodesOrIdents}")]
        [ProducesResponseType(typeof(IEnumerable<AirportDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AirportDto>>> GetAirportsByIcaoCodesOrIdents(string icaoCodesOrIdents)
        {
            var codesArray = icaoCodesOrIdents.Split(',')
                .Select(s => s.Trim())
                .ToArray();

            var airports = await airportService.GetAirportsByIcaoCodesOrIdents(codesArray);
            return Ok(airports);
        }

        /// <summary>
        /// Gets airports where ICAO code or identifier starts with the provided prefix
        /// </summary>
        /// <param name="prefix">The prefix to search for in ICAO codes or airport identifiers</param>
        /// <returns>List of airports with ICAO codes or identifiers starting with the provided prefix</returns>
        /// <response code="200">Returns the list of airports</response>
        [HttpGet("prefix/{prefix}")]
        [ProducesResponseType(typeof(IEnumerable<AirportDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AirportDto>>> GetAirportsByPrefix(string prefix)
        {
            var airports = await airportService.GetAirportsByPrefix(prefix);
            return Ok(airports);
        }

        /// <summary>
        /// Gets all runways for a specific airport
        /// </summary>
        /// <param name="icaoCodeOrIdent">ICAO code or airport identifier</param>
        /// <returns>List of runways with their runway ends</returns>
        /// <response code="200">Returns the list of runways</response>
        /// <response code="404">If the airport is not found</response>
        [HttpGet("{icaoCodeOrIdent}/runways")]
        [ProducesResponseType(typeof(IEnumerable<RunwayDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<RunwayDto>>> GetRunwaysByAirport(string icaoCodeOrIdent)
        {
            var runways = await runwayService.GetRunwaysByAirportAsync(icaoCodeOrIdent);
            return Ok(runways);
        }
    }
}
