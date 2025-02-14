using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using VFR3D.Infrastructure.Settings;

namespace VFR3D.API.Authentication;

public class Auth0Handler
{
    public static void ConfigureJwtBearer(JwtBearerOptions options, Auth0Settings? auth0Settings)
    {
        if (auth0Settings != null)
        {
            options.Authority = auth0Settings.Auth0Domain;
            options.Audience = auth0Settings.Auth0ApiIdentifier;
        }
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "name",
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                return Task.CompletedTask;
            }
        };
    }
}