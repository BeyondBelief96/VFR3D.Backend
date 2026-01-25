using Microsoft.AspNetCore.Mvc;
using VFR3D.API.Authentication;
using VFR3D.Domain.Enums;
using VFR3D.Infrastructure.Dtos;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ConditionalAuth]
public class AirsigmetController(IAirsigmetService airsigmetService, ILogger<AirsigmetController> logger)
    : ControllerBase
{
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
            var airsigmets = await airsigmetService.GetAllAirsigmets();
            return Ok(airsigmets);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving all AIRSIGMETs");
            return StatusCode(500, "An error occurred while retrieving AIRSIGMETs");
        }
    }

    /// <summary>
    /// Gets AIRSIGMETs by hazard type
    /// </summary>
    /// <param name="hazardType">Hazard type: CONVECTIVE, ICE, TURB, IFR, or MTN_OBSCN</param>
    /// <returns>List of AIRSIGMETs for the specified hazard type</returns>
    /// <response code="200">Returns the list of AIRSIGMETs</response>
    /// <response code="400">If the hazard type is invalid</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("hazard/{hazardType}")]
    [ProducesResponseType(typeof(IEnumerable<AirsigmetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<AirsigmetDto>>> GetAirsigmetsByHazardType(string hazardType)
    {
        try
        {
            if (!Enum.TryParse<AirsigmetHazardType>(hazardType, ignoreCase: true, out var hazardTypeEnum))
            {
                return BadRequest($"Invalid hazard type '{hazardType}'. Valid values are: CONVECTIVE, ICE, TURB, IFR, MTN_OBSCN");
            }

            var airsigmets = await airsigmetService.GetAirsigmetsByHazardType(hazardTypeEnum);
            return Ok(airsigmets);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving AIRSIGMETs for hazard type {HazardType}", hazardType);
            return StatusCode(500, "An error occurred while retrieving AIRSIGMETs");
        }
    }

    /// <summary>
    /// Gets all CONVECTIVE AIRSIGMETs (thunderstorms)
    /// </summary>
    /// <returns>List of CONVECTIVE AIRSIGMETs</returns>
    /// <response code="200">Returns the list of CONVECTIVE AIRSIGMETs</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("convective")]
    [ProducesResponseType(typeof(IEnumerable<AirsigmetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<AirsigmetDto>>> GetConvectiveAirsigmets()
    {
        try
        {
            var airsigmets = await airsigmetService.GetAirsigmetsByHazardType(AirsigmetHazardType.CONVECTIVE);
            return Ok(airsigmets);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving CONVECTIVE AIRSIGMETs");
            return StatusCode(500, "An error occurred while retrieving CONVECTIVE AIRSIGMETs");
        }
    }

    /// <summary>
    /// Gets all ICE AIRSIGMETs (icing conditions)
    /// </summary>
    /// <returns>List of ICE AIRSIGMETs</returns>
    /// <response code="200">Returns the list of ICE AIRSIGMETs</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("ice")]
    [ProducesResponseType(typeof(IEnumerable<AirsigmetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<AirsigmetDto>>> GetIceAirsigmets()
    {
        try
        {
            var airsigmets = await airsigmetService.GetAirsigmetsByHazardType(AirsigmetHazardType.ICE);
            return Ok(airsigmets);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving ICE AIRSIGMETs");
            return StatusCode(500, "An error occurred while retrieving ICE AIRSIGMETs");
        }
    }

    /// <summary>
    /// Gets all TURB AIRSIGMETs (turbulence)
    /// </summary>
    /// <returns>List of TURB AIRSIGMETs</returns>
    /// <response code="200">Returns the list of TURB AIRSIGMETs</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("turb")]
    [ProducesResponseType(typeof(IEnumerable<AirsigmetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<AirsigmetDto>>> GetTurbAirsigmets()
    {
        try
        {
            var airsigmets = await airsigmetService.GetAirsigmetsByHazardType(AirsigmetHazardType.TURB);
            return Ok(airsigmets);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving TURB AIRSIGMETs");
            return StatusCode(500, "An error occurred while retrieving TURB AIRSIGMETs");
        }
    }

    /// <summary>
    /// Gets all IFR AIRSIGMETs (low visibility/ceiling)
    /// </summary>
    /// <returns>List of IFR AIRSIGMETs</returns>
    /// <response code="200">Returns the list of IFR AIRSIGMETs</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("ifr")]
    [ProducesResponseType(typeof(IEnumerable<AirsigmetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<AirsigmetDto>>> GetIfrAirsigmets()
    {
        try
        {
            var airsigmets = await airsigmetService.GetAirsigmetsByHazardType(AirsigmetHazardType.IFR);
            return Ok(airsigmets);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving IFR AIRSIGMETs");
            return StatusCode(500, "An error occurred while retrieving IFR AIRSIGMETs");
        }
    }

    /// <summary>
    /// Gets all MTN OBSCN AIRSIGMETs (mountain obscuration)
    /// </summary>
    /// <returns>List of MTN OBSCN AIRSIGMETs</returns>
    /// <response code="200">Returns the list of MTN OBSCN AIRSIGMETs</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("mtn-obscn")]
    [ProducesResponseType(typeof(IEnumerable<AirsigmetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<AirsigmetDto>>> GetMtnObscnAirsigmets()
    {
        try
        {
            var airsigmets = await airsigmetService.GetAirsigmetsByHazardType(AirsigmetHazardType.MTN_OBSCN);
            return Ok(airsigmets);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving MTN OBSCN AIRSIGMETs");
            return StatusCode(500, "An error occurred while retrieving MTN OBSCN AIRSIGMETs");
        }
    }
}