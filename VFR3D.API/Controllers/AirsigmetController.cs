using Microsoft.AspNetCore.Mvc;
using VFR3D.API.Authentication;
using VFR3D.API.Models;
using VFR3D.Domain.Enums;
using VFR3D.Domain.Exceptions;
using VFR3D.Infrastructure.Dtos;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ConditionalAuth]
public class AirsigmetController(IAirsigmetService airsigmetService)
    : ControllerBase
{
    /// <summary>
    /// Gets all AIRSIGMETs
    /// </summary>
    /// <returns>List of all AIRSIGMETs</returns>
    /// <response code="200">Returns the list of AIRSIGMETs</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AirsigmetDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AirsigmetDto>>> GetAllAirsigmets()
    {
        var airsigmets = await airsigmetService.GetAllAirsigmets();
        return Ok(airsigmets);
    }

    /// <summary>
    /// Gets AIRSIGMETs by hazard type
    /// </summary>
    /// <param name="hazardType">Hazard type: CONVECTIVE, ICE, TURB, IFR, or MTN_OBSCN</param>
    /// <returns>List of AIRSIGMETs for the specified hazard type</returns>
    /// <response code="200">Returns the list of AIRSIGMETs</response>
    /// <response code="400">If the hazard type is invalid</response>
    [HttpGet("hazard/{hazardType}")]
    [ProducesResponseType(typeof(IEnumerable<AirsigmetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<AirsigmetDto>>> GetAirsigmetsByHazardType(string hazardType)
    {
        if (!Enum.TryParse<AirsigmetHazardType>(hazardType, ignoreCase: true, out var hazardTypeEnum))
        {
            throw new ValidationException("hazardType", $"Invalid hazard type '{hazardType}'. Valid values are: CONVECTIVE, ICE, TURB, IFR, MTN_OBSCN");
        }

        var airsigmets = await airsigmetService.GetAirsigmetsByHazardType(hazardTypeEnum);
        return Ok(airsigmets);
    }

    /// <summary>
    /// Gets all CONVECTIVE AIRSIGMETs (thunderstorms)
    /// </summary>
    /// <returns>List of CONVECTIVE AIRSIGMETs</returns>
    /// <response code="200">Returns the list of CONVECTIVE AIRSIGMETs</response>
    [HttpGet("convective")]
    [ProducesResponseType(typeof(IEnumerable<AirsigmetDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AirsigmetDto>>> GetConvectiveAirsigmets()
    {
        var airsigmets = await airsigmetService.GetAirsigmetsByHazardType(AirsigmetHazardType.CONVECTIVE);
        return Ok(airsigmets);
    }

    /// <summary>
    /// Gets all ICE AIRSIGMETs (icing conditions)
    /// </summary>
    /// <returns>List of ICE AIRSIGMETs</returns>
    /// <response code="200">Returns the list of ICE AIRSIGMETs</response>
    [HttpGet("ice")]
    [ProducesResponseType(typeof(IEnumerable<AirsigmetDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AirsigmetDto>>> GetIceAirsigmets()
    {
        var airsigmets = await airsigmetService.GetAirsigmetsByHazardType(AirsigmetHazardType.ICE);
        return Ok(airsigmets);
    }

    /// <summary>
    /// Gets all TURB AIRSIGMETs (turbulence)
    /// </summary>
    /// <returns>List of TURB AIRSIGMETs</returns>
    /// <response code="200">Returns the list of TURB AIRSIGMETs</response>
    [HttpGet("turb")]
    [ProducesResponseType(typeof(IEnumerable<AirsigmetDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AirsigmetDto>>> GetTurbAirsigmets()
    {
        var airsigmets = await airsigmetService.GetAirsigmetsByHazardType(AirsigmetHazardType.TURB);
        return Ok(airsigmets);
    }

    /// <summary>
    /// Gets all IFR AIRSIGMETs (low visibility/ceiling)
    /// </summary>
    /// <returns>List of IFR AIRSIGMETs</returns>
    /// <response code="200">Returns the list of IFR AIRSIGMETs</response>
    [HttpGet("ifr")]
    [ProducesResponseType(typeof(IEnumerable<AirsigmetDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AirsigmetDto>>> GetIfrAirsigmets()
    {
        var airsigmets = await airsigmetService.GetAirsigmetsByHazardType(AirsigmetHazardType.IFR);
        return Ok(airsigmets);
    }

    /// <summary>
    /// Gets all MTN OBSCN AIRSIGMETs (mountain obscuration)
    /// </summary>
    /// <returns>List of MTN OBSCN AIRSIGMETs</returns>
    /// <response code="200">Returns the list of MTN OBSCN AIRSIGMETs</response>
    [HttpGet("mtn-obscn")]
    [ProducesResponseType(typeof(IEnumerable<AirsigmetDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AirsigmetDto>>> GetMtnObscnAirsigmets()
    {
        var airsigmets = await airsigmetService.GetAirsigmetsByHazardType(AirsigmetHazardType.MTN_OBSCN);
        return Ok(airsigmets);
    }
}
