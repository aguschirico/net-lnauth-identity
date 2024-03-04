using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NetCore.Identity.LnAuth.Api.Domain.Entities;

namespace NetCore.Identity.LnAuth.Api.Database;

public class AppDbContext : IdentityDbContext<AppUser, AppRole, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> optionsBuilderOptions) : base(
        optionsBuilderOptions)
    {
    }

    public DbSet<LightningAuthLinkingKey> LinkingKeys { get; set; } = null!;
    
    
        
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        ConfigureIdentityTables(builder);
    }

    private static void ConfigureIdentityTables(ModelBuilder builder)
    {
        // Identity tables go into Identity Schema
        builder.Entity<AppUser>().ToTable("AspNetUsers", "identity");
        builder.Entity<AppRole>().ToTable("AspNetRoles", "identity");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("AspNetUserClaims", "identity");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("AspNetUserRoles", "identity");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("AspNetUserLogins", "identity");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("AspNetRoleClaims", "identity");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("AspNetUserTokens", "identity");
    }
}