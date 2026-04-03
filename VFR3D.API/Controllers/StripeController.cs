using Microsoft.AspNetCore.Mvc;
using VFR3D.API.Authentication;
using VFR3D.API.Models;
using VFR3D.Domain.Exceptions;
using VFR3D.Infrastructure.Dtos.Stripe;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.API.Controllers;

//[ApiController]
//[Route("api/[controller]")]
//[ConditionalAuth]
//public class StripeController(IStripeService stripeService)
//    : ControllerBase
//{
//    /// <summary>
//    /// Creates a subscription checkout session
//    /// </summary>
//    [HttpPost("[action]")]
//    [ProducesResponseType(typeof(StripeSessionResponseDto), StatusCodes.Status200OK)]
//    public async Task<ActionResult<StripeSessionResponseDto>> CreateSubscriptionCheckoutSession(
//        [FromBody] SubscriptionSessionRequestDto request)
//    {
//        var response = await stripeService.CreateSubscriptionCheckoutSession(
//            request.Auth0UserId,
//            request.Email);
//        return Ok(response);
//    }

//    /// <summary>
//    /// Creates a billing portal session
//    /// </summary>
//    [HttpPost("[action]")]
//    [ProducesResponseType(typeof(StripeUrlResponseDto), StatusCodes.Status200OK)]
//    public async Task<ActionResult<StripeUrlResponseDto>> CreatePortalSession(
//        [FromBody] CreatePortalSessionRequestDto request)
//    {
//        var response = await stripeService.CreatePortalSession(
//            request.Auth0UserId,
//            request.Email);
//        return Ok(response);
//    }

//    /// <summary>
//    /// Gets subscription details for a user
//    /// </summary>
//    [HttpGet("[action]/{auth0UserId}")]
//    [ProducesResponseType(typeof(StripeSubscriptionDto), StatusCodes.Status200OK)]
//    public async Task<ActionResult<StripeSubscriptionDto?>> GetSubscriptionDetails(
//        string auth0UserId,
//        [FromQuery] string email)
//    {
//        var subscription = await stripeService.GetSubscriptionDetails(auth0UserId, email);
//        return Ok(subscription);
//    }

//    /// <summary>
//    /// Cancels a subscription
//    /// </summary>
//    [HttpPost("[action]")]
//    [ProducesResponseType(StatusCodes.Status200OK)]
//    public async Task<IActionResult> CancelSubscription(
//        [FromBody] SubscriptionSessionRequestDto request)
//    {
//        await stripeService.CancelSubscription(request.Auth0UserId, request.Email);
//        return Ok();
//    }

//    /// <summary>
//    /// Reactivates a subscription
//    /// </summary>
//    [HttpPost("[action]")]
//    [ProducesResponseType(typeof(StripeReactivateSubscriptionResponseDto), StatusCodes.Status200OK)]
//    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
//    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
//    public async Task<ActionResult<StripeReactivateSubscriptionResponseDto>> ReactivateSubscription(
//        [FromBody] SubscriptionSessionRequestDto request)
//    {
//        var response = await stripeService.ReactivateSubscription(request.Auth0UserId, request.Email);
//        return Ok(response);
//    }
//}
