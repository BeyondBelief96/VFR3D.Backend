using Microsoft.AspNetCore.Mvc;
using VFR3D.API.Authentication;
using VFR3D.Infrastructure.Dtos;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ConditionalAuth]
    public class PirepController : ControllerBase
    {
        private readonly IPirepService _pirepService;
        private readonly ILogger<PirepController> _logger;

        public PirepController(IPirepService pirepService, ILogger<PirepController> logger)
        {
            _pirepService = pirepService;
            _logger = logger;
        }

        /// <summary>
        /// Gets all PIREPs
        /// </summary>
        /// <returns>List of all PIREPs</returns>
        /// <response code="200">Returns the list of PIREPs</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PirepDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<PirepDto>>> GetAllPireps()
        {
            try
            {
                var pireps = await _pirepService.GetAllPireps();
                return Ok(pireps);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all PIREPs");
                return StatusCode(500, "An error occurred while retrieving PIREPs");
            }
        }
    }
}
