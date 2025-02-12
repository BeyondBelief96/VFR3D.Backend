using Microsoft.AspNetCore.Mvc;
using VFR3D.Infrastructure.Dtos;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommunicationFrequencyController : ControllerBase
    {
        private readonly ICommunicationFrequencyService _frequencyService;
        private readonly ILogger<CommunicationFrequencyController> _logger;

        public CommunicationFrequencyController(
            ICommunicationFrequencyService frequencyService,
            ILogger<CommunicationFrequencyController> logger)
        {
            _frequencyService = frequencyService;
            _logger = logger;
        }

        /// <summary>
        /// Gets all communication frequencies for a specific serviced facility
        /// </summary>
        /// <param name="servicedFacility">The serviced facility identifier</param>
        /// <returns>List of communication frequencies for the facility</returns>
        /// <response code="200">Returns the list of communication frequencies</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("{servicedFacility}")]
        [ProducesResponseType(typeof(IEnumerable<CommunicationFrequencyDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<CommunicationFrequencyDto>>> GetFrequenciesByServicedFacility(
            string servicedFacility)
        {
            try
            {
                var frequencies = await _frequencyService.GetFrequenciesByServicedFacility(servicedFacility);
                return Ok(frequencies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving frequencies for facility: {ServicedFacility}", 
                    servicedFacility);
                return StatusCode(500, "An error occurred while retrieving communication frequencies");
            }
        }
    }
}