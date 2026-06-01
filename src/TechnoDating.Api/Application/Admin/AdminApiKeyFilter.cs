using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TechnoDating.Api.Application.Admin;

/// <summary>
/// Gates the admin endpoints with a shared key sent as the <c>X-Admin-Key</c> header — the
/// local-only stopgap while the admin tool runs on a trusted machine. Upgrade to an Identity
/// <c>Admin</c> role (roles are already wired) when the tool is deployed for a team.
/// </summary>
public class AdminApiKeyFilter(IConfiguration configuration) : IAsyncActionFilter
{
    public const string HeaderName = "X-Admin-Key";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var configured = configuration["Admin:ApiKey"];
        if (string.IsNullOrWhiteSpace(configured))
        {
            context.Result = new ObjectResult(new { error = "admin_not_configured" }) { StatusCode = StatusCodes.Status503ServiceUnavailable };
            return;
        }

        if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var provided)
            || !string.Equals(provided, configured, StringComparison.Ordinal))
        {
            context.Result = new UnauthorizedObjectResult(new { error = "admin_unauthorized" });
            return;
        }

        await next();
    }
}
