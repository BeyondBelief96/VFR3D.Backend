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
public class GAirmetController(IGAirmetService gairmetService)
    : ControllerBase
{
    /// <summary>
    /// Gets all G-AIRMETs
    /// </summary>
    /// <returns>List of all G-AIRMETs</returns>
    /// <response code="200">Returns the list of G-AIRMETs</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<GAirmetDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<GAirmetDto>>> GetAllGAirmets()
    {
        var gairmets = await gairmetService.GetAllGAirmets();
        return Ok(gairmets);
    }

    /// <summary>
    /// Gets G-AIRMETs by product type
    /// </summary>
    /// <param name="product">Product type: SIERRA, TANGO, or ZULU</param>
    /// <returns>List of G-AIRMETs for the specified product type</returns>
    /// <response code="200">Returns the list of G-AIRMETs</response>
    /// <response code="400">If the product type is invalid</response>
    [HttpGet("{product}")]
    [ProducesResponseType(typeof(IEnumerable<GAirmetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<GAirmetDto>>> GetGAirmetsByProduct(string product)
    {
        if (!Enum.TryParse<GAirmetProduct>(product, ignoreCase: true, out var productEnum))
        {
            throw new ValidationException("product", $"Invalid product type '{product}'. Valid values are: SIERRA, TANGO, ZULU");
        }

        var gairmets = await gairmetService.GetGAirmetsByProduct(productEnum);
        return Ok(gairmets);
    }

    /// <summary>
    /// Gets all SIERRA G-AIRMETs (IFR conditions and Mountain Obscuration)
    /// </summary>
    /// <returns>List of SIERRA G-AIRMETs</returns>
    /// <response code="200">Returns the list of SIERRA G-AIRMETs</response>
    [HttpGet("sierra")]
    [ProducesResponseType(typeof(IEnumerable<GAirmetDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<GAirmetDto>>> GetSierraGAirmets()
    {
        var gairmets = await gairmetService.GetGAirmetsByProduct(GAirmetProduct.SIERRA);
        return Ok(gairmets);
    }

    /// <summary>
    /// Gets all TANGO G-AIRMETs (Turbulence and sustained surface winds)
    /// </summary>
    /// <returns>List of TANGO G-AIRMETs</returns>
    /// <response code="200">Returns the list of TANGO G-AIRMETs</response>
    [HttpGet("tango")]
    [ProducesResponseType(typeof(IEnumerable<GAirmetDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<GAirmetDto>>> GetTangoGAirmets()
    {
        var gairmets = await gairmetService.GetGAirmetsByProduct(GAirmetProduct.TANGO);
        return Ok(gairmets);
    }

    /// <summary>
    /// Gets all ZULU G-AIRMETs (Icing and freezing levels)
    /// </summary>
    /// <returns>List of ZULU G-AIRMETs</returns>
    /// <response code="200">Returns the list of ZULU G-AIRMETs</response>
    [HttpGet("zulu")]
    [ProducesResponseType(typeof(IEnumerable<GAirmetDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<GAirmetDto>>> GetZuluGAirmets()
    {
        var gairmets = await gairmetService.GetGAirmetsByProduct(GAirmetProduct.ZULU);
        return Ok(gairmets);
    }

    // ==================== Hazard Type Endpoints ====================

    /// <summary>
    /// Gets G-AIRMETs by hazard type
    /// </summary>
    /// <param name="hazardType">Hazard type: MT_OBSC, IFR, TURB_LO, TURB_HI, LLWS, SFC_WIND, ICE, FZLVL, M_FZLVL</param>
    /// <returns>List of G-AIRMETs for the specified hazard type</returns>
    /// <response code="200">Returns the list of G-AIRMETs</response>
    /// <response code="400">If the hazard type is invalid</response>
    [HttpGet("hazard/{hazardType}")]
    [ProducesResponseType(typeof(IEnumerable<GAirmetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<GAirmetDto>>> GetGAirmetsByHazardType(string hazardType)
    {
        if (!Enum.TryParse<GAirmetHazardType>(hazardType, ignoreCase: true, out var hazardTypeEnum))
        {
            throw new ValidationException("hazardType", $"Invalid hazard type '{hazardType}'. Valid values are: MT_OBSC, IFR, TURB_LO, TURB_HI, LLWS, SFC_WIND, ICE, FZLVL, M_FZLVL");
        }

        var gairmets = await gairmetService.GetGAirmetsByHazardType(hazardTypeEnum);
        return Ok(gairmets);
    }

    // SIERRA Hazard Types

    /// <summary>
    /// Gets all MT_OBSC G-AIRMETs (Mountain Obscuration)
    /// </summary>
    /// <returns>List of Mountain Obscuration G-AIRMETs</returns>
    /// <response code="200">Returns the list of MT_OBSC G-AIRMETs</response>
    [HttpGet("hazard/mt-obsc")]
    [ProducesResponseType(typeof(IEnumerable<GAirmetDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<GAirmetDto>>> GetMtObscGAirmets()
    {
        var gairmets = await gairmetService.GetGAirmetsByHazardType(GAirmetHazardType.MT_OBSC);
        return Ok(gairmets);
    }

    /// <summary>
    /// Gets all IFR G-AIRMETs (IFR conditions)
    /// </summary>
    /// <returns>List of IFR G-AIRMETs</returns>
    /// <response code="200">Returns the list of IFR G-AIRMETs</response>
    [HttpGet("hazard/ifr")]
    [ProducesResponseType(typeof(IEnumerable<GAirmetDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<GAirmetDto>>> GetIfrGAirmets()
    {
        var gairmets = await gairmetService.GetGAirmetsByHazardType(GAirmetHazardType.IFR);
        return Ok(gairmets);
    }

    // TANGO Hazard Types

    /// <summary>
    /// Gets all TURB-LO G-AIRMETs (Low-level turbulence below FL180)
    /// </summary>
    /// <returns>List of low-level turbulence G-AIRMETs</returns>
    /// <response code="200">Returns the list of TURB-LO G-AIRMETs</response>
    [HttpGet("hazard/turb-lo")]
    [ProducesResponseType(typeof(IEnumerable<GAirmetDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<GAirmetDto>>> GetTurbLoGAirmets()
    {
        var gairmets = await gairmetService.GetGAirmetsByHazardType(GAirmetHazardType.TURB_LO);
        return Ok(gairmets);
    }

    /// <summary>
    /// Gets all TURB-HI G-AIRMETs (High-level turbulence at or above FL180)
    /// </summary>
    /// <returns>List of high-level turbulence G-AIRMETs</returns>
    /// <response code="200">Returns the list of TURB-HI G-AIRMETs</response>
    [HttpGet("hazard/turb-hi")]
    [ProducesResponseType(typeof(IEnumerable<GAirmetDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<GAirmetDto>>> GetTurbHiGAirmets()
    {
        var gairmets = await gairmetService.GetGAirmetsByHazardType(GAirmetHazardType.TURB_HI);
        return Ok(gairmets);
    }

    /// <summary>
    /// Gets all LLWS G-AIRMETs (Low-level wind shear)
    /// </summary>
    /// <returns>List of low-level wind shear G-AIRMETs</returns>
    /// <response code="200">Returns the list of LLWS G-AIRMETs</response>
    [HttpGet("hazard/llws")]
    [ProducesResponseType(typeof(IEnumerable<GAirmetDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<GAirmetDto>>> GetLlwsGAirmets()
    {
        var gairmets = await gairmetService.GetGAirmetsByHazardType(GAirmetHazardType.LLWS);
        return Ok(gairmets);
    }

    /// <summary>
    /// Gets all SFC_WIND G-AIRMETs (Strong surface winds 30+ knots)
    /// </summary>
    /// <returns>List of surface wind G-AIRMETs</returns>
    /// <response code="200">Returns the list of SFC_WIND G-AIRMETs</response>
    [HttpGet("hazard/sfc-wind")]
    [ProducesResponseType(typeof(IEnumerable<GAirmetDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<GAirmetDto>>> GetSfcWindGAirmets()
    {
        var gairmets = await gairmetService.GetGAirmetsByHazardType(GAirmetHazardType.SFC_WIND);
        return Ok(gairmets);
    }

    // ZULU Hazard Types

    /// <summary>
    /// Gets all ICE G-AIRMETs (Moderate icing)
    /// </summary>
    /// <returns>List of icing G-AIRMETs</returns>
    /// <response code="200">Returns the list of ICE G-AIRMETs</response>
    [HttpGet("hazard/ice")]
    [ProducesResponseType(typeof(IEnumerable<GAirmetDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<GAirmetDto>>> GetIceGAirmets()
    {
        var gairmets = await gairmetService.GetGAirmetsByHazardType(GAirmetHazardType.ICE);
        return Ok(gairmets);
    }

    /// <summary>
    /// Gets all FZLVL G-AIRMETs (Freezing level)
    /// </summary>
    /// <returns>List of freezing level G-AIRMETs</returns>
    /// <response code="200">Returns the list of FZLVL G-AIRMETs</response>
    [HttpGet("hazard/fzlvl")]
    [ProducesResponseType(typeof(IEnumerable<GAirmetDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<GAirmetDto>>> GetFzlvlGAirmets()
    {
        var gairmets = await gairmetService.GetGAirmetsByHazardType(GAirmetHazardType.FZLVL);
        return Ok(gairmets);
    }

    /// <summary>
    /// Gets all M_FZLVL G-AIRMETs (Multiple freezing levels)
    /// </summary>
    /// <returns>List of multiple freezing level G-AIRMETs</returns>
    /// <response code="200">Returns the list of M_FZLVL G-AIRMETs</response>
    [HttpGet("hazard/m-fzlvl")]
    [ProducesResponseType(typeof(IEnumerable<GAirmetDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<GAirmetDto>>> GetMFzlvlGAirmets()
    {
        var gairmets = await gairmetService.GetGAirmetsByHazardType(GAirmetHazardType.M_FZLVL);
        return Ok(gairmets);
    }
}
