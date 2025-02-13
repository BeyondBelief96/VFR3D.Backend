namespace VFR3D.Infrastructure.Dtos.Stripe;

public record StripeUrlResponseDto
{
    public string Url { get; set; } = string.Empty;
}