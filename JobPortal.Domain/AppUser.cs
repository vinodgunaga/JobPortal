using Microsoft.AspNetCore.Identity;

namespace JobPortal.Domain;

public class AppUser : IdentityUser
{
    public bool IsEmailVerified { get; set; } = false;
}