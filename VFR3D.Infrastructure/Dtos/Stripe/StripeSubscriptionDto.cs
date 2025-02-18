namespace VFR3D.Infrastructure.Dtos.Stripe;

public record StripeSubscriptionDto
{
    public string Id { get; init; } = string.Empty;
    public StripeSubscriptionStatus Status { get; init; }
    public DateTime CurrentPeriodEnd { get; init; }
    public bool CancelAtPeriodEnd { get; init; }
    public DateTime? TrialEnd { get; init; }
}