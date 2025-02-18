namespace VFR3D.Infrastructure.Dtos.Stripe
{
    public record StripeReactivateSubscriptionResponseDto
    {
        public StripeSubscriptionStatus Status { get; init; }
        public DateTime CurrentPeriodEnd { get; init; }
        public bool RequiresPayment { get; init; }
    }
}