using Daab.Modules.Auth.Models;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Auth.Persistence;

public class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; init; }
    public DbSet<Role> Roles { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var user = modelBuilder.Entity<User>();
        var role = modelBuilder.Entity<Role>();

        user.HasMany(u => u.Roles).WithMany(r => r.Users);
        user.HasIndex(u => u.Username);

        role.HasIndex(r => r.Name);
    }
}
