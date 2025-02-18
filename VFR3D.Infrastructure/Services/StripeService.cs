using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using VFR3D.Infrastructure.Dtos.Stripe;
using VFR3D.Infrastructure.Interfaces;
using VFR3D.Infrastructure.Settings;
using VFR3D.Infrastructure.Utilities;

namespace VFR3D.Infrastructure.Services;

public class StripeService : IStripeService
{
    private readonly ILogger<StripeService> _logger;
    private readonly string? _stripeMonthlySubscriptionPriceId;
    private readonly string? _domain;

    public StripeService(
        IOptions<StripeSettings> stripeSettings,
        ILogger<StripeService> logger)
    {
        _logger = logger;
        _stripeMonthlySubscriptionPriceId = stripeSettings.Value.StripeMonthlySubscriptionPriceId;
        _domain = stripeSettings.Value.Domain;
        StripeConfiguration.ApiKey = stripeSettings.Value.StripeSecretKey;
    }

    public async Task<StripeSessionResponseDto> CreateSubscriptionCheckoutSession(string auth0UserId, string email)
    {
        try
        {
            var customer = await GetOrCreateCustomer(auth0UserId, email);
            var hasHadTrial = await CheckIfCustomerHadTrial(customer.Id);

            var options = new Stripe.Checkout.SessionCreateOptions()
            {
                UiMode = "embedded",
                Customer = customer.Id,
                PaymentMethodTypes = ["card"],
                LineItems = new List<SessionLineItemOptions>
                {
                    new()
                    {
                        Price = _stripeMonthlySubscriptionPriceId,
                        Quantity = 1
                    }
                },
                Mode = "subscription",
                ReturnUrl = $"{_domain}/redirect",
            };

            if (!hasHadTrial)
            {
                options.SubscriptionData = new SessionSubscriptionDataOptions
                {
                    TrialPeriodDays = 30,
                    TrialSettings = new SessionSubscriptionDataTrialSettingsOptions
                    {
                        EndBehavior = new SessionSubscriptionDataTrialSettingsEndBehaviorOptions
                        {
                            MissingPaymentMethod = "cancel"
                        }
                    }
                };
            }

            options.PaymentMethodCollection = hasHadTrial ? "always" : "if_required";

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            if (session.ClientSecret == null)
            {
                throw new Exception("Stripe session created without client_secret");
            }

            return new StripeSessionResponseDto
            {
                ClientSecret = session.ClientSecret
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating subscription checkout session for user {Auth0UserId}", auth0UserId);
            throw;
        }
    }

    public async Task<StripeUrlResponseDto> CreatePortalSession(string auth0UserId, string email)
    {
        try
        {
            var customer = await GetOrCreateCustomer(auth0UserId, email);

            var options = new Stripe.BillingPortal.SessionCreateOptions
            {
                Customer = customer.Id,
                ReturnUrl = $"{_domain}/subscription"
            };

            var service = new Stripe.BillingPortal.SessionService();
            var session = await service.CreateAsync(options);

            return new StripeUrlResponseDto
            {
                Url = session.Url
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating portal session for user {Auth0UserId}", auth0UserId);
            throw;
        }
    }

    public async Task<StripeSubscriptionDto?> GetSubscriptionDetails(string auth0UserId, string email)
    {
        try
        {
            var customer = await GetOrCreateCustomer(auth0UserId, email);
            var service = new SubscriptionService();
            var subscriptions = await service.ListAsync(new SubscriptionListOptions
            {
                Customer = customer.Id,
                Status = "all",
                Limit = 1
            });

            var subscription = subscriptions.FirstOrDefault();
            if (subscription == null) return null;

            return new StripeSubscriptionDto
            {
                Id = subscription.Id,
                Status = ParseSubscriptionStatus(subscription.Status),
                CurrentPeriodEnd = subscription.CurrentPeriodEnd.ToUniversalTime(),
                CancelAtPeriodEnd = subscription.CancelAtPeriodEnd,
                TrialEnd = subscription.TrialEnd?.ToUniversalTime(),
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription details for user {Auth0UserId}", auth0UserId);
            throw;
        }
    }

    public async Task CancelSubscription(string auth0UserId, string email)
    {
        try
        {
            var customer = await GetOrCreateCustomer(auth0UserId, email);
            var service = new SubscriptionService();
            var subscriptions = await service.ListAsync(new SubscriptionListOptions
            {
                Customer = customer.Id,
                Status = "active",
                Limit = 1
            });

            var subscription = subscriptions.FirstOrDefault();
            if (subscription == null)
            {
                _logger.LogWarning("No active subscription found for user {Auth0UserId}", auth0UserId);
                return;
            }

            await service.UpdateAsync(subscription.Id, new SubscriptionUpdateOptions
            {
                CancelAtPeriodEnd = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error canceling subscription for user {Auth0UserId}", auth0UserId);
            throw;
        }
    }

    public async Task<StripeReactivateSubscriptionResponseDto> ReactivateSubscription(string auth0UserId, string email)
    {
        try
        {
            var customer = await GetOrCreateCustomer(auth0UserId, email);
            var service = new SubscriptionService();
            var subscriptions = await service.ListAsync(new SubscriptionListOptions
            {
                Customer = customer.Id,
                Status = "all",
                Limit = 1
            });

            var subscription = subscriptions.FirstOrDefault();
            if (subscription == null)
            {
                _logger.LogWarning("No subscription found for user {Auth0UserId}", auth0UserId);
                throw new KeyNotFoundException("Subscription not found");
            }

            var status = ParseSubscriptionStatus(subscription.Status);
            if ((status is StripeSubscriptionStatus.Active or StripeSubscriptionStatus.Trialing) && 
                subscription.CancelAtPeriodEnd)
            {
                var updatedSubscription = await service.UpdateAsync(subscription.Id, 
                    new SubscriptionUpdateOptions
                    {
                        CancelAtPeriodEnd = false
                    });

                return new StripeReactivateSubscriptionResponseDto
                {
                    Status = ParseSubscriptionStatus(updatedSubscription.Status),
                    CurrentPeriodEnd = updatedSubscription.CurrentPeriodEnd.ToUniversalTime(),
                    RequiresPayment = false
                };
            }
            else if (status == StripeSubscriptionStatus.Canceled)
            {
                var newSubscription = await service.CreateAsync(new SubscriptionCreateOptions
                {
                    Customer = customer.Id,
                    Items = subscription.Items.Data.Select(item => new SubscriptionItemOptions
                    {
                        Price = item.Price.Id
                    }).ToList(),
                    TrialEnd = subscription.TrialEnd
                });

                return new StripeReactivateSubscriptionResponseDto
                {
                    Status = ParseSubscriptionStatus(newSubscription.Status),
                    CurrentPeriodEnd = newSubscription.CurrentPeriodEnd.ToUniversalTime(),
                    RequiresPayment = true
                };
            }

            throw new InvalidOperationException("Subscription cannot be reactivated");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reactivating subscription for user {Auth0UserId}", auth0UserId);
            throw;
        }
    }

    private async Task<Customer> GetOrCreateCustomer(string auth0UserId, string email)
    {
        var service = new CustomerService();
        var existingCustomers = await service.ListAsync(new CustomerListOptions
        {
            Email = email,
            Limit = 1
        });

        if (existingCustomers.Any())
        {
            var existingCustomer = existingCustomers.First();
            if (existingCustomer.Metadata.TryGetValue("auth0UserId", out var storedAuth0UserId) && 
                storedAuth0UserId != auth0UserId)
            {
                await service.UpdateAsync(existingCustomer.Id, new CustomerUpdateOptions
                {
                    Metadata = new Dictionary<string, string>
                    {
                        { "auth0UserId", auth0UserId }
                    }
                });
            }
            return existingCustomer;
        }

        return await service.CreateAsync(new CustomerCreateOptions
        {
            Email = email,
            Metadata = new Dictionary<string, string>
            {
                { "auth0UserId", auth0UserId }
            }
        });
    }

    private async Task<bool> CheckIfCustomerHadTrial(string customerId)
    {
        var service = new SubscriptionService();
        var subscriptions = await service.ListAsync(new SubscriptionListOptions
        {
            Customer = customerId,
            Status = "all"
        });

        return subscriptions.Any(s => s.TrialEnd.HasValue);
    }
    
    private static StripeSubscriptionStatus ParseSubscriptionStatus(string status)
    {
        return status.ToLower() switch
        {
            "active" => StripeSubscriptionStatus.Active,
            "trialing" => StripeSubscriptionStatus.Trialing,
            "canceled" => StripeSubscriptionStatus.Canceled,
            "past_due" => StripeSubscriptionStatus.PastDue,
            "unpaid" => StripeSubscriptionStatus.Unpaid,
            "paused" => StripeSubscriptionStatus.Paused,
            "incomplete" => StripeSubscriptionStatus.Incomplete,
            "incomplete_expired" => StripeSubscriptionStatus.IncompleteExpired,
            _ => throw new ArgumentException($"Unknown subscription status: {status}")
        };
    }
}