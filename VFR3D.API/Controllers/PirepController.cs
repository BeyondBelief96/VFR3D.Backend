using Microsoft.AspNetCore.Mvc;
using VFR3D.API.Authentication;
using VFR3D.Infrastructure.Dtos;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ConditionalAuth]
    public class PirepController(IPirepService pirepService, ILogger<PirepController> logger)
        : ControllerBase
    {
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
                var pireps = await pirepService.GetAllPireps();
                return Ok(pireps);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving all PIREPs");
                return StatusCode(500, "An error occurred while retrieving PIREPs");
            }
        }
    }
}
