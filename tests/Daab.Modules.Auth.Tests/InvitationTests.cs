using Daab.Modules.Auth.Common;
using Daab.Modules.Auth.Features.Invitations.AcceptInvitation;
using Daab.Modules.Auth.Features.Login;
using Daab.Modules.Auth.Models;
using Daab.Modules.Auth.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Auth.Tests;

public sealed class InvitationTests
{
    private const string Password = "Secret123";

    [Fact]
    public async Task Invite_CreatesAPendingAccount_ThatCannotSignIn()
    {
        await using var context = NewContext();

        var invite = await InviteAsync(context, "Ada@Example.com");

        Assert.False(invite.AlreadyActive);
        Assert.NotNull(invite.Token);
        var user = context.Users.Single();
        Assert.Equal("ada@example.com", user.Email);
        Assert.False(user.HasPassword);
        Assert.Contains(user.Roles, r => r.Name == AuthRoles.User);
        Assert.Equal(401, await LoginStatusAsync(context, "ada@example.com", ""));
    }

    [Fact]
    public async Task Accept_SetsThePassword_AndTheLinkCannotBeReused()
    {
        await using var context = NewContext();
        var invite = await InviteAsync(context, "ada@example.com");
        var handler = new AcceptInvitationCommandHandler(context);

        var first = await handler.Handle(
            new AcceptInvitationCommand(invite.Token!, Password, Password),
            CancellationToken.None
        );
        var second = await handler.Handle(
            new AcceptInvitationCommand(invite.Token!, "Other1234", "Other1234"),
            CancellationToken.None
        );

        Assert.True(first.IsSucc);
        Assert.Equal(404, second.Match(_ => 0, e => e.Code));
        Assert.Equal(0, await LoginStatusAsync(context, "ada@example.com", Password));
    }

    [Theory]
    [InlineData("short1", "short1")]
    [InlineData("lettersonly", "lettersonly")]
    [InlineData("12345678", "12345678")]
    [InlineData("Secret123", "Secret124")]
    public async Task Accept_RejectsWeakOrMismatchedPasswords_AndKeepsTheLinkUsable(
        string password,
        string confirm
    )
    {
        await using var context = NewContext();
        var invite = await InviteAsync(context, "ada@example.com");
        var handler = new AcceptInvitationCommandHandler(context);

        var result = await handler.Handle(
            new AcceptInvitationCommand(invite.Token!, password, confirm),
            CancellationToken.None
        );

        Assert.Equal(400, result.Match(_ => 0, e => e.Code));
        Assert.True(
            (
                await handler.Handle(new GetInvitationQuery(invite.Token!), CancellationToken.None)
            ).IsSucc
        );
    }

    [Fact]
    public async Task Reinvite_ReplacesTheEarlierLink()
    {
        await using var context = NewContext();
        var first = await InviteAsync(context, "ada@example.com");
        var second = await InviteAsync(context, "ada@example.com");
        var handler = new AcceptInvitationCommandHandler(context);

        Assert.Equal(first.UserId, second.UserId);
        Assert.Single(context.Users);
        Assert.True(
            (
                await handler.Handle(new GetInvitationQuery(first.Token!), CancellationToken.None)
            ).IsFail
        );
        Assert.True(
            (
                await handler.Handle(new GetInvitationQuery(second.Token!), CancellationToken.None)
            ).IsSucc
        );
    }

    [Fact]
    public async Task ExpiredLinks_AreRejected()
    {
        await using var context = NewContext();
        var invite = await InviteAsync(context, "ada@example.com");
        var stored = context.Invitations.Single();
        context.Entry(stored).Property(i => i.ExpiresAt).CurrentValue = DateTime.UtcNow.AddMinutes(
            -1
        );
        await context.SaveChangesAsync();

        var result = await new AcceptInvitationCommandHandler(context).Handle(
            new AcceptInvitationCommand(invite.Token!, Password, Password),
            CancellationToken.None
        );

        Assert.True(result.IsFail);
    }

    [Fact]
    public async Task Invite_ForAnAccountWithAPassword_IssuesNoLink()
    {
        await using var context = NewContext();
        var existing = User.Register("ada@example.com", "Ada", Password);
        context.Users.Add(existing);
        await context.SaveChangesAsync();

        var invite = await InviteAsync(context, "ada@example.com");

        Assert.True(invite.AlreadyActive);
        Assert.Equal(existing.Id, invite.UserId);
        Assert.Empty(context.Invitations);
    }

    [Fact]
    public async Task Tokens_AreStoredOnlyAsHashes()
    {
        await using var context = NewContext();
        var invite = await InviteAsync(context, "ada@example.com");

        var stored = context.Invitations.Single();
        Assert.NotEqual(invite.Token, stored.TokenHash);
        Assert.Equal(Invitation.Hash(invite.Token!), stored.TokenHash);
    }

    private static AuthDbContext NewContext()
    {
        var context = new AuthDbContext(
            new DbContextOptionsBuilder<AuthDbContext>()
                .UseInMemoryDatabase($"auth_{Guid.NewGuid():N}")
                .Options
        );
        context.Roles.Add(new Role(AuthRoles.User));
        context.SaveChanges();
        return context;
    }

    private static async Task<InviteResult> InviteAsync(AuthDbContext context, string email)
    {
        var result = await new InvitationService(context).InviteAsync(
            new InviteRequest(email, "Ada Lovelace", null),
            CancellationToken.None
        );
        return result.Match(r => r, e => throw new Xunit.Sdk.XunitException(e.Message));
    }

    /// <returns>0 when the sign-in succeeds, otherwise the error status.</returns>
    private static async Task<int> LoginStatusAsync(
        AuthDbContext context,
        string username,
        string password
    )
    {
        var result = await new LoginCommandHandler(context).Handle(
            new LoginCommand(new LoginRequest(username, password)),
            CancellationToken.None
        );
        return result.Match(_ => 0, e => e.Code);
    }
}
