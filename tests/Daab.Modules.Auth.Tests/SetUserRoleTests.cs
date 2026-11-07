using Daab.Modules.Auth.Common;
using Daab.Modules.Auth.Features.Users.SetUserRole;
using Daab.Modules.Auth.Models;

namespace Daab.Modules.Auth.Tests;

public sealed class SetUserRoleTests
{
    [Fact]
    public async Task GrantingAndRemovingScientist_ChangesTheStoredRoles()
    {
        await using var context = TestDb.New();
        var user = User.Register("ada@example.com", "Ada", "Secret123");
        context.Users.Add(user);
        await context.SaveChangesAsync();
        var handler = new SetUserRoleCommandHandler(context);

        var granted = await handler.Handle(
            new SetUserRoleCommand(user.Id, AuthRoles.Scientist, Granted: true),
            CancellationToken.None
        );
        var again = await handler.Handle(
            new SetUserRoleCommand(user.Id, AuthRoles.Scientist, Granted: true),
            CancellationToken.None
        );
        Assert.Equal([AuthRoles.Scientist], granted.Match(r => r.Roles, _ => []));
        Assert.Equal([AuthRoles.Scientist], again.Match(r => r.Roles, _ => []));

        var removed = await handler.Handle(
            new SetUserRoleCommand(user.Id, AuthRoles.Scientist, Granted: false),
            CancellationToken.None
        );
        Assert.Empty(removed.Match(r => r.Roles, _ => ["failed"]));
    }

    [Theory]
    [InlineData(AuthRoles.Admin)]
    [InlineData("superuser")]
    public async Task OnlyAssignableRoles_CanBeChanged(string role)
    {
        await using var context = TestDb.New();
        var user = User.Register("ada@example.com", "Ada", "Secret123");
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var result = await new SetUserRoleCommandHandler(context).Handle(
            new SetUserRoleCommand(user.Id, role, Granted: true),
            CancellationToken.None
        );

        Assert.Equal(400, result.Match(_ => 0, e => e.Code));
    }

    [Fact]
    public async Task UnknownUser_IsNotFound()
    {
        await using var context = TestDb.New();

        var result = await new SetUserRoleCommandHandler(context).Handle(
            new SetUserRoleCommand("nope", AuthRoles.Scientist, Granted: true),
            CancellationToken.None
        );

        Assert.Equal(404, result.Match(_ => 0, e => e.Code));
    }
}
