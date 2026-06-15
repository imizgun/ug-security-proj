using Microsoft.AspNetCore.Identity;

namespace backend.Infrastructure;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
}
