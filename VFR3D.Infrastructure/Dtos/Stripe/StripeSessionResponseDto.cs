namespace VFR3D.Infrastructure.Dtos.Stripe;

public record StripeSessionResponseDto
{
    public string ClientSecret { get; set; } = string.Empty;
}