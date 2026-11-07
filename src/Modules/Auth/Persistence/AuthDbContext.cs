using Daab.Modules.Auth.Models;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Auth.Persistence;

public class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; init; }
    public DbSet<Role> Roles { get; init; }
    public DbSet<RefreshToken> RefreshTokens { get; init; }
    public DbSet<Invitation> Invitations { get; init; }
    public DbSet<ExternalLogin> ExternalLogins { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var user = modelBuilder.Entity<User>();
        var role = modelBuilder.Entity<Role>();
        var refreshToken = modelBuilder.Entity<RefreshToken>();
        var invitation = modelBuilder.Entity<Invitation>();
        var externalLogin = modelBuilder.Entity<ExternalLogin>();

        user.HasKey(u => u.Id);
        user.HasMany(u => u.Roles).WithMany(r => r.Users);
        user.HasIndex(u => u.Username);
        user.HasIndex(u => u.Email);

        role.HasIndex(r => r.Name);

        refreshToken.HasKey(rt => rt.Id);
        refreshToken
            .HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId);

        invitation.HasKey(i => i.Id);
        invitation.HasIndex(i => i.TokenHash).IsUnique();
        invitation
            .HasOne(i => i.User)
            .WithMany()
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        externalLogin.HasKey(l => l.Id);
        externalLogin.HasIndex(l => new { l.Provider, l.ProviderKey }).IsUnique();
        externalLogin
            .HasOne(l => l.User)
            .WithMany(u => u.ExternalLogins)
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
