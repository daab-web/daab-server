using Daab.Modules.Auth.Models;
using Daab.Modules.Auth.Persistence;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Auth.Features.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Fin<User>>
{
    private readonly AuthDbContext _context;

    public LoginCommandHandler(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<Fin<User>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _context
            .Users.Include(u => u.Roles)
            .AsNoTracking()
            .SingleOrDefaultAsync(
                u => u.Username == request.Username,
                cancellationToken: cancellationToken
            );

        if (user is null)
        {
            return Error.New(StatusCodes.Status404NotFound, "Requested user does not exist");
        }

        var hasher = new PasswordHasher<User>();

        PasswordVerificationResult result = hasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password
        );

        if (result is PasswordVerificationResult.Failed)
        {
            return Error.New(StatusCodes.Status401Unauthorized, "Invalid credentials");
        }

        return user;
    }
}
