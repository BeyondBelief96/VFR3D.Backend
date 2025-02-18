using VFR3D.Infrastructure.Dtos.Stripe;

namespace VFR3D.Infrastructure.Interfaces;

public interface IStripeService
{
    Task<StripeSessionResponseDto> CreateSubscriptionCheckoutSession(string auth0UserId, string email);
    Task<StripeUrlResponseDto> CreatePortalSession(string auth0UserId, string email);
    Task<StripeSubscriptionDto?> GetSubscriptionDetails(string auth0UserId, string email);
    Task CancelSubscription(string auth0UserId, string email);
    Task<StripeReactivateSubscriptionResponseDto> ReactivateSubscription(string auth0UserId, string email);
}
