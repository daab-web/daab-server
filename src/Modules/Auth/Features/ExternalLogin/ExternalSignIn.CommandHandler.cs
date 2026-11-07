using Daab.Modules.Auth.Common;
using Daab.Modules.Auth.Persistence;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ExternalLoginEntity = Daab.Modules.Auth.Models.ExternalLogin;
using User = Daab.Modules.Auth.Models.User;

namespace Daab.Modules.Auth.Features.ExternalLogin;

public sealed class ExternalSignInCommandHandler(
    AuthDbContext context,
    ILogger<ExternalSignInCommandHandler> logger
) : IRequestHandler<ExternalSignInCommand, Fin<User>>
{
    public async Task<Fin<User>> Handle(
        ExternalSignInCommand request,
        CancellationToken cancellationToken
    )
    {
        // 1. Returning user: this provider account is already linked.
        var login = await context
            .ExternalLogins.Include(l => l.User)
                .ThenInclude(u => u.Roles)
            .SingleOrDefaultAsync(
                l => l.Provider == request.Provider && l.ProviderKey == request.ProviderKey,
                cancellationToken
            );

        if (login is not null)
        {
            login.User.FillProfileFrom(request.FullName, request.PictureUrl);
            await context.SaveChangesAsync(cancellationToken);
            return login.User;
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return Error.New(
                StatusCodes.Status400BadRequest,
                "The provider did not share an e-mail address"
            );
        }

        var email = User.NormalizeEmail(request.Email);
        var user = await context
            .Users.Include(u => u.Roles)
            .SingleOrDefaultAsync(u => u.Username == email || u.Email == email, cancellationToken);

        if (user is not null)
        {
            // 2. Existing account with the same e-mail: attach the provider only when it has
            //    verified the address, otherwise anyone could claim someone else's account.
            if (!request.EmailVerified)
            {
                return Error.New(
                    StatusCodes.Status409Conflict,
                    "An account with this e-mail already exists. Sign in with your password."
                );
            }

            user.FillProfileFrom(request.FullName, request.PictureUrl);
            logger.LogInformation(
                "Linked {Provider} sign-in to existing user {UserId}",
                request.Provider,
                user.Id
            );
        }
        else
        {
            // 3. New visitor: create the account from the provider's profile.
            user = User.FromExternal(email, request.FullName, request.PictureUrl);
            var userRole = await context.Roles.SingleOrDefaultAsync(
                r => r.Name == AuthRoles.User,
                cancellationToken
            );
            if (userRole is not null)
            {
                user.Roles.Add(userRole);
            }

            context.Users.Add(user);
            logger.LogInformation(
                "Created user {UserId} from {Provider} sign-in",
                user.Id,
                request.Provider
            );
        }

        context.ExternalLogins.Add(
            new ExternalLoginEntity
            {
                Provider = request.Provider,
                ProviderKey = request.ProviderKey,
                UserId = user.Id,
            }
        );
        await context.SaveChangesAsync(cancellationToken);

        return user;
    }
}
