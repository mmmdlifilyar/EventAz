using Microsoft.AspNetCore.Identity;

namespace EventAz.Persistence.Identity;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = default!;
}
