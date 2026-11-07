using Daab.Modules.Auth.Common;
using Daab.Modules.Auth.Features.ExternalLogin;
using Daab.Modules.Auth.Models;
using Daab.Modules.Auth.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Daab.Modules.Auth.Tests;

public sealed class ExternalSignInTests
{
    [Fact]
    public async Task FirstGoogleSignIn_CreatesARegularAccount_FromTheGoogleProfile()
    {
        await using var context = TestDb.New();

        var user = await SignInAsync(context, Google("sub-1", "Grace@Example.com"));

        Assert.Equal("grace@example.com", user.Email);
        Assert.Equal("Grace Hopper", user.FullName);
        Assert.Equal("https://example.com/grace.png", user.PictureUrl);
        Assert.False(user.HasPassword);
        Assert.Equal([AuthRoles.User], user.Roles.Select(r => r.Name));
        Assert.Single(context.ExternalLogins);
    }

    [Fact]
    public async Task ReturningGoogleUser_SignsInToTheSameAccount()
    {
        await using var context = TestDb.New();

        var first = await SignInAsync(context, Google("sub-1", "grace@example.com"));
        var second = await SignInAsync(context, Google("sub-1", "grace@example.com"));

        Assert.Equal(first.Id, second.Id);
        Assert.Single(context.Users);
        Assert.Single(context.ExternalLogins);
    }

    [Fact]
    public async Task VerifiedEmail_LinksToTheExistingAccount_AndKeepsItsRoles()
    {
        await using var context = TestDb.New();
        var existing = User.Register("grace@example.com", "Grace", "Secret123");
        existing.Roles.Add(context.Roles.Single(r => r.Name == AuthRoles.Scientist));
        context.Users.Add(existing);
        await context.SaveChangesAsync();

        var user = await SignInAsync(context, Google("sub-1", "grace@example.com"));

        Assert.Equal(existing.Id, user.Id);
        Assert.Single(context.Users);
        Assert.Contains(user.Roles, r => r.Name == AuthRoles.Scientist);
        Assert.Equal("Grace", user.FullName);
    }

    [Fact]
    public async Task UnverifiedEmail_IsNotAttachedToAnExistingAccount()
    {
        await using var context = TestDb.New();
        context.Users.Add(User.Register("grace@example.com", "Grace", "Secret123"));
        await context.SaveChangesAsync();

        var result = await Handler(context)
            .Handle(
                Google("sub-1", "grace@example.com") with
                {
                    EmailVerified = false,
                },
                CancellationToken.None
            );

        Assert.Equal(409, result.Match(_ => 0, e => e.Code));
        Assert.Empty(context.ExternalLogins);
    }

    [Fact]
    public async Task MissingEmail_IsRejected()
    {
        await using var context = TestDb.New();

        var result = await Handler(context).Handle(Google("sub-1", null), CancellationToken.None);

        Assert.True(result.IsFail);
        Assert.Empty(context.Users);
    }

    private static ExternalSignInCommand Google(string sub, string? email) =>
        new(
            ExternalLogin.Google,
            sub,
            email,
            EmailVerified: true,
            "Grace Hopper",
            "https://example.com/grace.png"
        );

    private static ExternalSignInCommandHandler Handler(AuthDbContext context) =>
        new(context, NullLogger<ExternalSignInCommandHandler>.Instance);

    private static async Task<User> SignInAsync(
        AuthDbContext context,
        ExternalSignInCommand command
    )
    {
        var result = await Handler(context).Handle(command, CancellationToken.None);
        return result.Match(u => u, e => throw new Xunit.Sdk.XunitException(e.Message));
    }
}

internal static class TestDb
{
    public static AuthDbContext New()
    {
        var context = new AuthDbContext(
            new DbContextOptionsBuilder<AuthDbContext>()
                .UseInMemoryDatabase($"auth_{Guid.NewGuid():N}")
                .Options
        );
        context.Roles.AddRange(
            new Role(AuthRoles.Admin),
            new Role(AuthRoles.User),
            new Role(AuthRoles.Scientist)
        );
        context.SaveChanges();
        return context;
    }
}
