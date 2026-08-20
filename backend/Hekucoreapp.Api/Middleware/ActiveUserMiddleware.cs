using Hekucoreapp.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Hekucoreapp.Api.Middleware;

public class ActiveUserMiddleware
{
    private readonly RequestDelegate _next;

    public ActiveUserMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                var user = await userManager.FindByIdAsync(userId);
                if (user == null || !user.IsActive)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsJsonAsync(new { message = "Account is deactivated." });
                    return;
                }
            }
        }

        await _next(context);
    }
}