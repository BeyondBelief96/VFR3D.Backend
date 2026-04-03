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
    public class PirepController(IPirepService pirepService)
        : ControllerBase
    {
        /// <summary>
        /// Gets all PIREPs
        /// </summary>
        /// <returns>List of all PIREPs</returns>
        /// <response code="200">Returns the list of PIREPs</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PirepDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PirepDto>>> GetAllPireps()
        {
            var pireps = await pirepService.GetAllPireps();
            return Ok(pireps);
        }
    }
}
