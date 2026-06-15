using backend.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace backend.Infrastructure;

public class DataSeeder(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IOpenIddictApplicationManager appManager,
    AppDbContext db)
{
    public async Task SeedAsync()
    {
        await SeedRolesAsync();
        var moderator = await SeedUserAsync("moderator@ugsocial.local", "Mod User", "Mod123!", "moderator");
        var user = await SeedUserAsync("user@ugsocial.local", "Regular User", "User123!", "user");
        await SeedClientAsync();
        await SeedPostsAsync(moderator, user);
    }

    private async Task SeedRolesAsync()
    {
        foreach (var role in new[] { "moderator", "user" })
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
    }

    private async Task<ApplicationUser> SeedUserAsync(string email, string displayName, string password, string role)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing is null)
        {
            var newUser = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                DisplayName = displayName
            };
            await userManager.CreateAsync(newUser, password);
            existing = newUser;
        }
        if (!await userManager.IsInRoleAsync(existing, role))
            await userManager.AddToRoleAsync(existing, role);
        return existing;
    }

    private async Task SeedClientAsync()
    {
        const string clientId = "angular-spa";
        if (await appManager.FindByClientIdAsync(clientId) is not null) return;

        await appManager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            ClientType = ClientTypes.Public,
            RedirectUris =
            {
                new Uri("http://localhost:4200/callback"),
                new Uri("http://localhost/callback")
            },
            PostLogoutRedirectUris =
            {
                new Uri("http://localhost:4200"),
                new Uri("http://localhost")
            },
            Permissions =
            {
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.Token,
                Permissions.Endpoints.Logout,
                Permissions.GrantTypes.AuthorizationCode,
                Permissions.ResponseTypes.Code,
                Permissions.Scopes.Email,
                Permissions.Scopes.Profile,
                Permissions.Prefixes.Scope + "roles"
            }
        });
    }

    private async Task SeedPostsAsync(ApplicationUser moderator, ApplicationUser user)
    {
        if (await db.Posts.AnyAsync()) return;

        db.Posts.AddRange(
            new Post
            {
                AuthorId = moderator.Id,
                AuthorName = moderator.DisplayName!,
                Content = "Welcome to UgSocial! Posts from moderators are marked with a badge.",
                CreatedAt = DateTime.UtcNow.AddMinutes(-30)
            },
            new Post
            {
                AuthorId = user.Id,
                AuthorName = user.DisplayName!,
                Content = "Hello everyone! Glad to be here.",
                CreatedAt = DateTime.UtcNow.AddMinutes(-20)
            },
            new Post
            {
                AuthorId = user.Id,
                AuthorName = user.DisplayName!,
                Content = "This is a great platform!",
                CreatedAt = DateTime.UtcNow.AddMinutes(-10)
            }
        );
        await db.SaveChangesAsync();
    }
}
