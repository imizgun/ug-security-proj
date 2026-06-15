using System.Security.Claims;
using backend.Domain;
using backend.Infrastructure;
using backend.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;

namespace backend.Endpoints;

public static class PostEndpoints
{
    public static void MapPostEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/posts").RequireAuthorization("api");

        group.MapGet("/", async (AppDbContext db, UserManager<ApplicationUser> userManager) =>
        {
            var posts = await db.Posts
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var moderatorIds = await ResolveModeratorsAsync(posts.Select(p => p.AuthorId).Distinct(), userManager);

            return Results.Ok(posts.Select(p => ToResponse(p, moderatorIds)));
        });

        group.MapGet("/{id:int}", async (int id, AppDbContext db, UserManager<ApplicationUser> userManager) =>
        {
            var post = await db.Posts.FindAsync(id);
            if (post is null) return Results.NotFound();

            var author = await userManager.FindByIdAsync(post.AuthorId);
            var isMod = author is not null && await userManager.IsInRoleAsync(author, "moderator");
            var singleMods = isMod ? new HashSet<string> { post.AuthorId } : [];

            return Results.Ok(ToResponse(post, singleMods));
        });

        group.MapPost("/", async (
            CreatePostRequest request,
            HttpContext ctx,
            AppDbContext db,
            UserManager<ApplicationUser> userManager,
            PostService postService) =>
        {
            var validation = postService.ValidateContent(request.Content);
            if (!validation.IsValid)
                return Results.BadRequest(new { error = validation.Error });

            var userId = ctx.User.GetClaim(OpenIddictConstants.Claims.Subject)
                ?? ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null) return Results.Unauthorized();

            var user = await userManager.FindByIdAsync(userId);
            if (user is null) return Results.Unauthorized();

            var post = new Post
            {
                Content = request.Content.Trim(),
                AuthorId = userId,
                AuthorName = user.DisplayName ?? user.Email ?? userId,
                CreatedAt = DateTime.UtcNow
            };

            db.Posts.Add(post);
            await db.SaveChangesAsync();

            var isMod = await userManager.IsInRoleAsync(user, "moderator");
            var mods = new HashSet<string>();
            if (isMod) mods.Add(userId);
            return Results.Created($"/api/posts/{post.Id}", ToResponse(post, mods));
        });

        group.MapDelete("/{id:int}", async (
            int id,
            HttpContext ctx,
            AppDbContext db,
            PostService postService) =>
        {
            var post = await db.Posts.FindAsync(id);
            if (post is null) return Results.NotFound();

            if (!postService.CanDelete(ctx.User.IsInRole("moderator")))
                return Results.Forbid();

            db.Posts.Remove(post);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization("moderator");
    }

    private static async Task<HashSet<string>> ResolveModeratorsAsync(
        IEnumerable<string> authorIds,
        UserManager<ApplicationUser> userManager)
    {
        var result = new HashSet<string>();
        foreach (var id in authorIds)
        {
            var u = await userManager.FindByIdAsync(id);
            if (u is not null && await userManager.IsInRoleAsync(u, "moderator"))
                result.Add(id);
        }
        return result;
    }

    private static PostResponse ToResponse(Post post, IReadOnlySet<string> moderatorIds) =>
        new(post.Id, post.Content, post.AuthorId, post.AuthorName,
            moderatorIds.Contains(post.AuthorId), post.CreatedAt);
}

record CreatePostRequest(string Content);
record PostResponse(int Id, string Content, string AuthorId, string AuthorName, bool IsModeratorPost, DateTime CreatedAt);
