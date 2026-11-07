using Daab.Modules.Auth.Common;
using Daab.Modules.Auth.Persistence;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Auth.Features.Users.SetUserRole;

public sealed class SetUserRoleCommandHandler(AuthDbContext context)
    : IRequestHandler<SetUserRoleCommand, Fin<SetUserRoleResponse>>
{
    public async Task<Fin<SetUserRoleResponse>> Handle(
        SetUserRoleCommand request,
        CancellationToken cancellationToken
    )
    {
        if (!AuthRoles.Assignable.Contains(request.Role))
        {
            return Error.New(
                StatusCodes.Status400BadRequest,
                $"The role '{request.Role}' cannot be assigned here"
            );
        }

        var user = await context
            .Users.Include(u => u.Roles)
            .SingleOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
        {
            return Error.New(StatusCodes.Status404NotFound, "User not found");
        }

        var current = user.Roles.SingleOrDefault(r => r.Name == request.Role);

        if (request.Granted && current is null)
        {
            var role = await context.Roles.SingleOrDefaultAsync(
                r => r.Name == request.Role,
                cancellationToken
            );
            if (role is null)
            {
                return Error.New(StatusCodes.Status500InternalServerError, "Role is not seeded");
            }

            user.Roles.Add(role);
        }
        else if (!request.Granted && current is not null)
        {
            user.Roles.Remove(current);
        }

        await context.SaveChangesAsync(cancellationToken);

        return new SetUserRoleResponse(
            user.Id,
            [.. user.Roles.Select(r => r.Name).Order(StringComparer.Ordinal)]
        );
    }
}
