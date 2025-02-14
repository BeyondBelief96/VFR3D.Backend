using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using VFR3D.Infrastructure.Settings;

namespace VFR3D.API.Authentication;

public class ConditionalAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IWebHostEnvironment _environment;
    private readonly Auth0Settings _auth0Settings;

    public ConditionalAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IWebHostEnvironment environment,
        IOptions<Auth0Settings> auth0Settings)
        : base(options, logger, encoder, clock)
    {
        _environment = environment;
        _auth0Settings = auth0Settings.Value;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
            if (_environment.IsDevelopment() && !_auth0Settings.RequireAuthenticationInDevelopment)
        {
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim("sub", "development-user"),
                new System.Security.Claims.Claim("email", "dev@example.com")
            };
            var identity = new System.Security.Claims.ClaimsIdentity(claims, Scheme.Name);
            var principal = new System.Security.Claims.ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        return Task.FromResult(AuthenticateResult.NoResult());
    }
}