using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
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
using Microsoft.IdentityModel.Tokens;

namespace Daab.Modules.Auth.Features.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Fin<LoginResponse>>
{
    private readonly JwtOptions _options;
    private readonly AuthDbContext _context;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(AuthDbContext context, IOptions<JwtOptions> options, ILogger<LoginCommandHandler> logger)
    {
        _context = context;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Fin<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Roles)
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Username == request.Username, cancellationToken: cancellationToken);

        if (user is null)
        {
            return Error.New(404, "Requested user does not exist");
        }

        var hasher = new PasswordHasher<User>();

        PasswordVerificationResult result = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (result is PasswordVerificationResult.Failed)
        {
            return Error.New(401, "Invalid credentials");
        }

        return new LoginResponse(GenerateAccessToken(user));
    }

    private string GenerateAccessToken(User user)
    {
        List<Claim> claims =
        [
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.Username),
        ];

        claims.AddRange(user.Roles.Select(r => new Claim(ClaimTypes.Role, r.Name)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.JwtSecret));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.ExpiresMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}