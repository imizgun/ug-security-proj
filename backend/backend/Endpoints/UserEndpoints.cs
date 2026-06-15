using backend.Infrastructure;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;

namespace backend.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        app.MapGet("/api/users/me", async (
            HttpContext context,
            UserManager<ApplicationUser> userManager) =>
        {
            var userId = context.User.GetClaim(OpenIddictConstants.Claims.Subject);
            if (userId is null) return Results.Unauthorized();

            var user = await userManager.FindByIdAsync(userId);
            if (user is null) return Results.NotFound();

            var roles = await userManager.GetRolesAsync(user);

            return Results.Ok(new
            {
                id = user.Id,
                email = user.Email,
                displayName = user.DisplayName,
                roles
            });
        }).RequireAuthorization("api");
    }
}
