using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using JobPortal.Domain;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<Job> Jobs { get; set; }

    public DbSet<Company> Companies { get; set; }

    public DbSet<JobApplication> Applications { get; set; }

    public DbSet<RefreshToken> RefreshTokens { get; set; }
}