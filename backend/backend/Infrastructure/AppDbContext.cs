using backend.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Post> Posts => Set<Post>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Post>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Content).HasMaxLength(500).IsRequired();
            e.Property(p => p.AuthorId).IsRequired();
            e.Property(p => p.AuthorName).HasMaxLength(256).IsRequired();
        });
    }
}
