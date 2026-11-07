using Daab.Modules.Auth.Common;
using Daab.Modules.Auth.Models;
using Daab.Modules.Auth.Persistence;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Auth.Features.Register;

public sealed class RegisterCommandHandler(AuthDbContext context)
    : IRequestHandler<RegisterCommand, Fin<RegisterResponse>>
{
    public async Task<Fin<RegisterResponse>> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken
    )
    {
        var email = User.NormalizeEmail(request.Email);

        var exists = await context.Users.AnyAsync(
            u => u.Username == email || u.Email == email,
            cancellationToken
        );

        if (exists)
        {
            return Error.New(
                StatusCodes.Status409Conflict,
                "An account with this email already exists"
            );
        }

        var userRole = await context.Roles.SingleOrDefaultAsync(
            r => r.Name == AuthRoles.User,
            cancellationToken
        );

        var user = User.Register(email, request.FullName, request.Password);
        if (userRole is not null)
        {
            user.Roles.Add(userRole);
        }

        await context.Users.AddAsync(user, cancellationToken);
        var written = await context.SaveChangesAsync(cancellationToken);

        return written <= 0
            ? Error.New(
                StatusCodes.Status500InternalServerError,
                "Unable to create the account. Please try again later"
            )
            : new RegisterResponse(user.Id, user.Email!, user.DisplayName);
    }
}
