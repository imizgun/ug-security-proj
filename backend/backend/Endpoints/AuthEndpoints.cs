using System.Security.Claims;
using backend.Infrastructure;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace backend.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/api/auth/login", async (
            LoginRequest request,
            HttpContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager) =>
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null)
                return Results.Unauthorized();

            var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
            if (!result.Succeeded)
                return Results.Unauthorized();

            await signInManager.SignInAsync(user, isPersistent: false);
            return Results.Ok(new { message = "ok" });
        });
        
        app.MapGet("/connect/authorize", async (
            HttpContext context,
            UserManager<ApplicationUser> userManager,
            IConfiguration config) =>
        {
            var request = context.GetOpenIddictServerRequest()
                ?? throw new InvalidOperationException("OpenIddict server request not found.");

            var cookieResult = await context.AuthenticateAsync(IdentityConstants.ApplicationScheme);
            if (!cookieResult.Succeeded)
            {
                var frontendUrl = config["Frontend:Url"] ?? "http://localhost:4200";
                var returnUrl = Uri.EscapeDataString(context.Request.Path + context.Request.QueryString);
                return Results.Redirect($"{frontendUrl}/login?returnUrl={returnUrl}");
            }

            var userId = cookieResult.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await userManager.FindByIdAsync(userId!);
            if (user is null) return Results.Challenge();

            var roles = await userManager.GetRolesAsync(user);

            var identity = new ClaimsIdentity(
                authenticationType: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                nameType: OpenIddictConstants.Claims.Name,
                roleType: OpenIddictConstants.Claims.Role);

            identity.SetClaim(OpenIddictConstants.Claims.Subject, user.Id);
            identity.SetClaim(OpenIddictConstants.Claims.Email, user.Email ?? string.Empty);
            identity.SetClaim(OpenIddictConstants.Claims.Name, user.DisplayName ?? user.Email ?? string.Empty);
            identity.SetClaim(OpenIddictConstants.Claims.PreferredUsername, user.UserName ?? string.Empty);

            foreach (var role in roles)
                identity.AddClaim(new Claim(OpenIddictConstants.Claims.Role, role));

            var principal = new ClaimsPrincipal(identity);
            principal.SetScopes(request.GetScopes());
            principal.SetDestinations(claim => claim.Type switch
            {
                OpenIddictConstants.Claims.Subject or
                OpenIddictConstants.Claims.Email or
                OpenIddictConstants.Claims.Name or
                OpenIddictConstants.Claims.Role =>
                    [OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken],
                _ => [OpenIddictConstants.Destinations.AccessToken]
            });

            return Results.SignIn(principal, null, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        });

        app.MapPost("/connect/token", async (HttpContext context) =>
        {
            var request = context.GetOpenIddictServerRequest()
                ?? throw new InvalidOperationException("OpenIddict server request not found.");

            if (!request.IsAuthorizationCodeGrantType())
                return Results.Forbid(authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);

            var result = await context.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            if (!result.Succeeded)
                return Results.Forbid(authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);

            return Results.SignIn(result.Principal!, null, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        });

        app.MapGet("/connect/userinfo", async (HttpContext context) =>
        {
            var result = await context.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            if (!result.Succeeded) return Results.Unauthorized();

            var principal = result.Principal!;
            return Results.Ok(new
            {
                sub = principal.GetClaim(OpenIddictConstants.Claims.Subject),
                email = principal.GetClaim(OpenIddictConstants.Claims.Email),
                name = principal.GetClaim(OpenIddictConstants.Claims.Name),
                roles = principal.GetClaims(OpenIddictConstants.Claims.Role).ToArray()
            });
        });

        app.MapMethods("/connect/logout", ["GET", "POST"], async (HttpContext context) =>
        {
            await context.SignOutAsync(IdentityConstants.ApplicationScheme);
            return Results.SignOut(
                new AuthenticationProperties { RedirectUri = "/" },
                [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
        });
    }
}

record LoginRequest(string Email, string Password);
