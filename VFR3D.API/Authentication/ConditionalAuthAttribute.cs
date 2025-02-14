using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using VFR3D.Infrastructure.Settings;

namespace VFR3D.API.Authentication;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ConditionalAuthAttribute : AuthorizeAttribute, IAuthorizationFilter
{
    public ConditionalAuthAttribute()
    {
        AuthenticationSchemes = "Conditional," + JwtBearerDefaults.AuthenticationScheme;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var environment = context.HttpContext.RequestServices
            .GetRequiredService<IWebHostEnvironment>();
        var auth0Settings = context.HttpContext.RequestServices
            .GetRequiredService<IOptions<Auth0Settings>>().Value;

        // Only bypass auth if we're in development AND not requiring auth
        if (environment.IsDevelopment() && !auth0Settings.RequireAuthenticationInDevelopment)
        {
            return;
        }

        var user = context.HttpContext.User;
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
        }
    }
}
