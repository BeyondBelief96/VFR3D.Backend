using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VFR3D.API.Authentication;
using VFR3D.Infrastructure.Dtos.Stripe;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.API.Controllers;

//[ApiController]
//[Route("api/[controller]")]
//[ConditionalAuth]
//public class StripeController(
//    IStripeService stripeService,
//    ILogger<StripeController> logger)
//    : ControllerBase
//{
//    /// <summary>
//    /// Creates a subscription checkout session
//    /// </summary>
//    [HttpPost("[action]")]
//    [ProducesResponseType(typeof(StripeSessionResponseDto), StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
//    public async Task<ActionResult<StripeSessionResponseDto>> CreateSubscriptionCheckoutSession(
//        [FromBody] SubscriptionSessionRequestDto request)
//    {
//        try
//        {
//            var response = await stripeService.CreateSubscriptionCheckoutSession(
//                request.Auth0UserId, 
//                request.Email);
//            return Ok(response);
//        }
//        catch (Exception ex)
//        {
//            logger.LogError(ex, "Error creating subscription checkout session for user {Auth0UserId}", 
//                request.Auth0UserId);
//            return StatusCode(500, "An error occurred while creating the checkout session");
//        }
//    }

//    /// <summary>
//    /// Creates a billing portal session
//    /// </summary>
//    [HttpPost("[action]")]
//    [ProducesResponseType(typeof(StripeUrlResponseDto), StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
//    public async Task<ActionResult<StripeUrlResponseDto>> CreatePortalSession(
//        [FromBody] CreatePortalSessionRequestDto request)
//    {
//        try
//        {
//            var response = await stripeService.CreatePortalSession(
//                request.Auth0UserId, 
//                request.Email);
//            return Ok(response);
//        }
//        catch (Exception ex)
//        {
//            logger.LogError(ex, "Error creating portal session for user {Auth0UserId}", 
//                request.Auth0UserId);
//            return StatusCode(500, "An error occurred while creating the portal session");
//        }
//    }

//    /// <summary>
//    /// Gets subscription details for a user
//    /// </summary>
//    [HttpGet("[action]/{auth0UserId}")]
//    [ProducesResponseType(typeof(StripeSubscriptionDto), StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
//    public async Task<ActionResult<StripeSubscriptionDto?>> GetSubscriptionDetails(
//        string auth0UserId,
//        [FromQuery] string email)
//    {
//        try
//        {
//            var subscription = await stripeService.GetSubscriptionDetails(auth0UserId, email);
//            return Ok(subscription);
//        }
//        catch (Exception ex)
//        {
//            logger.LogError(ex, "Error getting subscription details for user {Auth0UserId}", 
//                auth0UserId);
//            return StatusCode(500, "An error occurred while getting subscription details");
//        }
//    }

//    /// <summary>
//    /// Cancels a subscription
//    /// </summary>
//    [HttpPost("[action]")]
//    [ProducesResponseType(StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
//    public async Task<IActionResult> CancelSubscription(
//        [FromBody] SubscriptionSessionRequestDto request)
//    {
//        try
//        {
//            await stripeService.CancelSubscription(request.Auth0UserId, request.Email);
//            return Ok();
//        }
//        catch (Exception ex)
//        {
//            logger.LogError(ex, "Error canceling subscription for user {Auth0UserId}", 
//                request.Auth0UserId);
//            return StatusCode(500, "An error occurred while canceling the subscription");
//        }
//    }

//    /// <summary>
//    /// Reactivates a subscription
//    /// </summary>
//    [HttpPost("[action]")]
//    [ProducesResponseType(typeof(StripeReactivateSubscriptionResponseDto), StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status404NotFound)]
//    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
//    public async Task<ActionResult<StripeReactivateSubscriptionResponseDto>> ReactivateSubscription(
//        [FromBody] SubscriptionSessionRequestDto request)
//    {
//        try
//        {
//            var response = await stripeService.ReactivateSubscription(request.Auth0UserId, request.Email);
//            return Ok(response);
//        }
//        catch (KeyNotFoundException ex)
//        {
//            return NotFound(ex.Message);
//        }
//        catch (InvalidOperationException ex)
//        {
//            return BadRequest(ex.Message);
//        }
//        catch (Exception ex)
//        {
//            logger.LogError(ex, "Error reactivating subscription for user {Auth0UserId}", 
//                request.Auth0UserId);
//            return StatusCode(500, "An error occurred while reactivating the subscription");
//        }
//    }
//}