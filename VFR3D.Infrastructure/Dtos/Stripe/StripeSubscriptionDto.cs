namespace VFR3D.Infrastructure.Dtos.Stripe;

public record StripeSubscriptionDto
{
    public string Id { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CurrentPeriodEnd { get; set; }
    public bool CancelAtPeriodEnd { get; set; }
    public bool TrialEnd { get; set; }
}