using Daab.Modules.Auth.Common;
using Daab.Modules.Auth.Models;
using Daab.Modules.Auth.Options;
using Daab.Modules.Auth.Persistence;
using LanguageExt;
using LanguageExt.Common;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Daab.Modules.Auth.Features.Login;

public class LoginCommandHandler
    : IRequestHandler<LoginCommand, Fin<(LoginResponse, Models.RefreshToken refreshToken)>>
{
    private readonly JwtOptions _options;
    private readonly AuthDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        AuthDbContext context,
        ITokenService tokenService,
        IOptions<JwtOptions> options,
        ILogger<LoginCommandHandler> logger
    )
    {
        _context = context;
        _tokenService = tokenService;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Fin<(LoginResponse, Models.RefreshToken refreshToken)>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken
    )
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
            return Error.New(404, "Requested user does not exist");
        }

        var hasher = new PasswordHasher<User>();

        PasswordVerificationResult result = hasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password
        );

        if (result is PasswordVerificationResult.Failed)
        {
            return Error.New(401, "Invalid credentials");
        }

        var refreshToken = _tokenService.GenerateRefreshToken(user.Id);
        var accessToken = _tokenService.GenerateAccessToken(user);

        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return (new LoginResponse(accessToken), refreshToken);
    }
}
