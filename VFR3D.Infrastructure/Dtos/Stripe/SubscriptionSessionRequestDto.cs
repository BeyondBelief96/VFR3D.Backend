namespace VFR3D.Infrastructure.Dtos.Stripe;

public record SubscriptionSessionRequestDto
{
    public string Auth0UserId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}