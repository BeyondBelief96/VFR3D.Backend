using Microsoft.AspNetCore.Mvc;
using VFR3D.Infrastructure.Dtos;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AirsigmetController : ControllerBase
{
    private readonly IAirsigmetService _airsigmetService;
    private readonly ILogger<AirsigmetController> _logger;

    public AirsigmetController(IAirsigmetService airsigmetService, ILogger<AirsigmetController> logger)
    {
        _airsigmetService = airsigmetService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all AIRSIGMETs
    /// </summary>
    /// <returns>List of all AIRSIGMETs</returns>
    /// <response code="200">Returns the list of AIRSIGMETs</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AirsigmetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<AirsigmetDto>>> GetAllAirsigmets()
    {
        try
        {
            var airsigmets = await _airsigmetService.GetAllAirsigmets();
            return Ok(airsigmets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all AIRSIGMETs");
            return StatusCode(500, "An error occurred while retrieving AIRSIGMETs");
        }
    }
}