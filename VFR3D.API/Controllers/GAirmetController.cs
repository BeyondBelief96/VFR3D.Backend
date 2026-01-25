using Microsoft.AspNetCore.Mvc;
using VFR3D.API.Authentication;
using VFR3D.Domain.Enums;
using VFR3D.Infrastructure.Dtos;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ConditionalAuth]
public class GAirmetController(IGAirmetService gairmetService, ILogger<GAirmetController> logger)
    : ControllerBase
{
    /// <summary>
    /// Gets all G-AIRMETs
    /// </summary>
    /// <returns>List of all G-AIRMETs</returns>
    /// <response code="200">Returns the list of G-AIRMETs</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<GAirmetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<GAirmetDto>>> GetAllGAirmets()
    {
        try
        {
            var gairmets = await gairmetService.GetAllGAirmets();
            return Ok(gairmets);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving all G-AIRMETs");
            return StatusCode(500, "An error occurred while retrieving G-AIRMETs");
        }
    }

    /// <summary>
    /// Gets G-AIRMETs by product type
    /// </summary>
    /// <param name="product">Product type: SIERRA, TANGO, or ZULU</param>
    /// <returns>List of G-AIRMETs for the specified product type</returns>
    /// <response code="200">Returns the list of G-AIRMETs</response>
    /// <response code="400">If the product type is invalid</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("{product}")]
    [ProducesResponseType(typeof(IEnumerable<GAirmetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<GAirmetDto>>> GetGAirmetsByProduct(string product)
    {
        try
        {
            if (!Enum.TryParse<GAirmetProduct>(product, ignoreCase: true, out var productEnum))
            {
                return BadRequest($"Invalid product type '{product}'. Valid values are: SIERRA, TANGO, ZULU");
            }

            var gairmets = await gairmetService.GetGAirmetsByProduct(productEnum);
            return Ok(gairmets);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving G-AIRMETs for product {Product}", product);
            return StatusCode(500, "An error occurred while retrieving G-AIRMETs");
        }
    }

    /// <summary>
    /// Gets all SIERRA G-AIRMETs (IFR conditions and Mountain Obscuration)
    /// </summary>
    /// <returns>List of SIERRA G-AIRMETs</returns>
    /// <response code="200">Returns the list of SIERRA G-AIRMETs</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("sierra")]
    [ProducesResponseType(typeof(IEnumerable<GAirmetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<GAirmetDto>>> GetSierraGAirmets()
    {
        try
        {
            var gairmets = await gairmetService.GetGAirmetsByProduct(GAirmetProduct.SIERRA);
            return Ok(gairmets);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving SIERRA G-AIRMETs");
            return StatusCode(500, "An error occurred while retrieving SIERRA G-AIRMETs");
        }
    }

    /// <summary>
    /// Gets all TANGO G-AIRMETs (Turbulence and sustained surface winds)
    /// </summary>
    /// <returns>List of TANGO G-AIRMETs</returns>
    /// <response code="200">Returns the list of TANGO G-AIRMETs</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("tango")]
    [ProducesResponseType(typeof(IEnumerable<GAirmetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<GAirmetDto>>> GetTangoGAirmets()
    {
        try
        {
            var gairmets = await gairmetService.GetGAirmetsByProduct(GAirmetProduct.TANGO);
            return Ok(gairmets);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving TANGO G-AIRMETs");
            return StatusCode(500, "An error occurred while retrieving TANGO G-AIRMETs");
        }
    }

    /// <summary>
    /// Gets all ZULU G-AIRMETs (Icing and freezing levels)
    /// </summary>
    /// <returns>List of ZULU G-AIRMETs</returns>
    /// <response code="200">Returns the list of ZULU G-AIRMETs</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("zulu")]
    [ProducesResponseType(typeof(IEnumerable<GAirmetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<GAirmetDto>>> GetZuluGAirmets()
    {
        try
        {
            var gairmets = await gairmetService.GetGAirmetsByProduct(GAirmetProduct.ZULU);
            return Ok(gairmets);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving ZULU G-AIRMETs");
            return StatusCode(500, "An error occurred while retrieving ZULU G-AIRMETs");
        }
    }

    // ==================== Hazard Type Endpoints ====================

    /// <summary>
    /// Gets G-AIRMETs by hazard type
    /// </summary>
    /// <param name="hazardType">Hazard type: MT_OBSC, IFR, TURB_LO, TURB_HI, LLWS, SFC_WIND, ICE, FZLVL, M_FZLVL</param>
    /// <returns>List of G-AIRMETs for the specified hazard type</returns>
    /// <response code="200">Returns the list of G-AIRMETs</response>
    /// <response code="400">If the hazard type is invalid</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("hazard/{hazardType}")]
    [ProducesResponseType(typeof(IEnumerable<GAirmetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<GAirmetDto>>> GetGAirmetsByHazardType(string hazardType)
    {
        try
        {
            if (!Enum.TryParse<GAirmetHazardType>(hazardType, ignoreCase: true, out var hazardTypeEnum))
            {
                return BadRequest($"Invalid hazard type '{hazardType}'. Valid values are: MT_OBSC, IFR, TURB_LO, TURB_HI, LLWS, SFC_WIND, ICE, FZLVL, M_FZLVL");
            }

            var gairmets = await gairmetService.GetGAirmetsByHazardType(hazardTypeEnum);
            return Ok(gairmets);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving G-AIRMETs for hazard type {HazardType}", hazardType);
            return StatusCode(500, "An error occurred while retrieving G-AIRMETs");
        }
    }

    // SIERRA Hazard Types

    /// <summary>
    /// Gets all MT_OBSC G-AIRMETs (Mountain Obscuration)
    /// </summary>
    /// <returns>List of Mountain Obscuration G-AIRMETs</returns>
    /// <response code="200">Returns the list of MT_OBSC G-AIRMETs</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("hazard/mt-obsc")]
    [ProducesResponseType(typeof(IEnumerable<GAirmetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<GAirmetDto>>> GetMtObscGAirmets()
    {
        try
        {
            var gairmets = await gairmetService.GetGAirmetsByHazardType(GAirmetHazardType.MT_OBSC);
            return Ok(gairmets);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving MT_OBSC G-AIRMETs");
            return StatusCode(500, "An error occurred while retrieving MT_OBSC G-AIRMETs");
        }
    }

    /// <summary>
    /// Gets all IFR G-AIRMETs (IFR conditions)
    /// </summary>
    /// <returns>List of IFR G-AIRMETs</returns>
    /// <response code="200">Returns the list of IFR G-AIRMETs</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("hazard/ifr")]
    [ProducesResponseType(typeof(IEnumerable<GAirmetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<GAirmetDto>>> GetIfrGAirmets()
    {
        try
        {
            var gairmets = await gairmetService.GetGAirmetsByHazardType(GAirmetHazardType.IFR);
            return Ok(gairmets);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving IFR G-AIRMETs");
            return StatusCode(500, "An error occurred while retrieving IFR G-AIRMETs");
        }
    }

    // TANGO Hazard Types

    /// <summary>
    /// Gets all TURB-LO G-AIRMETs (Low-level turbulence below FL180)
    /// </summary>
    /// <returns>List of low-level turbulence G-AIRMETs</returns>
    /// <response code="200">Returns the list of TURB-LO G-AIRMETs</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("hazard/turb-lo")]
    [ProducesResponseType(typeof(IEnumerable<GAirmetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<GAirmetDto>>> GetTurbLoGAirmets()
    {
        try
        {
            var gairmets = await gairmetService.GetGAirmetsByHazardType(GAirmetHazardType.TURB_LO);
            return Ok(gairmets);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving TURB-LO G-AIRMETs");
            return StatusCode(500, "An error occurred while retrieving TURB-LO G-AIRMETs");
        }
    }

    /// <summary>
    /// Gets all TURB-HI G-AIRMETs (High-level turbulence at or above FL180)
    /// </summary>
    /// <returns>List of high-level turbulence G-AIRMETs</returns>
    /// <response code="200">Returns the list of TURB-HI G-AIRMETs</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("hazard/turb-hi")]
    [ProducesResponseType(typeof(IEnumerable<GAirmetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<GAirmetDto>>> GetTurbHiGAirmets()
    {
        try
        {
            var gairmets = await gairmetService.GetGAirmetsByHazardType(GAirmetHazardType.TURB_HI);
            return Ok(gairmets);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving TURB-HI G-AIRMETs");
            return StatusCode(500, "An error occurred while retrieving TURB-HI G-AIRMETs");
        }
    }

    /// <summary>
    /// Gets all LLWS G-AIRMETs (Low-level wind shear)
    /// </summary>
    /// <returns>List of low-level wind shear G-AIRMETs</returns>
    /// <response code="200">Returns the list of LLWS G-AIRMETs</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("hazard/llws")]
    [ProducesResponseType(typeof(IEnumerable<GAirmetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<GAirmetDto>>> GetLlwsGAirmets()
    {
        try
        {
            var gairmets = await gairmetService.GetGAirmetsByHazardType(GAirmetHazardType.LLWS);
            return Ok(gairmets);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving LLWS G-AIRMETs");
            return StatusCode(500, "An error occurred while retrieving LLWS G-AIRMETs");
        }
    }

    /// <summary>
    /// Gets all SFC_WIND G-AIRMETs (Strong surface winds 30+ knots)
    /// </summary>
    /// <returns>List of surface wind G-AIRMETs</returns>
    /// <response code="200">Returns the list of SFC_WIND G-AIRMETs</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("hazard/sfc-wind")]
    [ProducesResponseType(typeof(IEnumerable<GAirmetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<GAirmetDto>>> GetSfcWindGAirmets()
    {
        try
        {
            var gairmets = await gairmetService.GetGAirmetsByHazardType(GAirmetHazardType.SFC_WIND);
            return Ok(gairmets);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving SFC_WIND G-AIRMETs");
            return StatusCode(500, "An error occurred while retrieving SFC_WIND G-AIRMETs");
        }
    }

    // ZULU Hazard Types

    /// <summary>
    /// Gets all ICE G-AIRMETs (Moderate icing)
    /// </summary>
    /// <returns>List of icing G-AIRMETs</returns>
    /// <response code="200">Returns the list of ICE G-AIRMETs</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("hazard/ice")]
    [ProducesResponseType(typeof(IEnumerable<GAirmetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<GAirmetDto>>> GetIceGAirmets()
    {
        try
        {
            var gairmets = await gairmetService.GetGAirmetsByHazardType(GAirmetHazardType.ICE);
            return Ok(gairmets);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving ICE G-AIRMETs");
            return StatusCode(500, "An error occurred while retrieving ICE G-AIRMETs");
        }
    }

    /// <summary>
    /// Gets all FZLVL G-AIRMETs (Freezing level)
    /// </summary>
    /// <returns>List of freezing level G-AIRMETs</returns>
    /// <response code="200">Returns the list of FZLVL G-AIRMETs</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("hazard/fzlvl")]
    [ProducesResponseType(typeof(IEnumerable<GAirmetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<GAirmetDto>>> GetFzlvlGAirmets()
    {
        try
        {
            var gairmets = await gairmetService.GetGAirmetsByHazardType(GAirmetHazardType.FZLVL);
            return Ok(gairmets);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving FZLVL G-AIRMETs");
            return StatusCode(500, "An error occurred while retrieving FZLVL G-AIRMETs");
        }
    }

    /// <summary>
    /// Gets all M_FZLVL G-AIRMETs (Multiple freezing levels)
    /// </summary>
    /// <returns>List of multiple freezing level G-AIRMETs</returns>
    /// <response code="200">Returns the list of M_FZLVL G-AIRMETs</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("hazard/m-fzlvl")]
    [ProducesResponseType(typeof(IEnumerable<GAirmetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<GAirmetDto>>> GetMFzlvlGAirmets()
    {
        try
        {
            var gairmets = await gairmetService.GetGAirmetsByHazardType(GAirmetHazardType.M_FZLVL);
            return Ok(gairmets);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving M_FZLVL G-AIRMETs");
            return StatusCode(500, "An error occurred while retrieving M_FZLVL G-AIRMETs");
        }
    }
}
