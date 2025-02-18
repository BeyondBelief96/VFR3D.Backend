namespace VFR3D.Infrastructure.Dtos.Stripe;

public enum StripeSubscriptionStatus
{
    Active,
    Trialing,
    Canceled,
    PastDue,
    Unpaid,
    Paused,
    Incomplete,
    IncompleteExpired
}