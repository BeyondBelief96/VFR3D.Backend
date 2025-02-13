namespace VFR3D.Infrastructure.Dtos.Stripe;

public record CreateSubscriptionSessionRequestDto
{
    public string Auth0UserId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}