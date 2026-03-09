using System.Net;
using System.Security.Claims;
using Daab.Modules.Auth.Models;
using Daab.Modules.Auth.Options;
using Daab.Modules.Auth.Persistence;
using FastEndpoints;
using FastEndpoints.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Daab.Modules.Auth.Common;

public interface ITokenService
{
    RefreshToken GenerateRefreshToken(string userId);
    string GenerateAccessToken(User user);
}

public class TokenService : RefreshTokenService<TokenRequest, TokenResponse>
{
    private readonly AuthDbContext _context;
    private readonly JwtOptions _options;

    public TokenService(AuthDbContext context, IOptions<JwtOptions> options)
    {
        _context = context;
        _options = options.Value;

        Setup(x =>
        {
            x.TokenSigningKey = _options.JwtSecret;
            x.AccessTokenValidity = TimeSpan.FromMinutes(_options.ExpiresMinutes);
            x.RefreshTokenValidity = TimeSpan.FromDays(7);
            x.Issuer = _options.Issuer;
            x.Audience = _options.Audience;
            x.Endpoint("/auth/refresh-token", ep => ep.Tags("Auth"));
        });
    }

    public override async Task PersistTokenAsync(TokenResponse response)
    {
        _context.RefreshTokens.Add(
            new RefreshToken
            {
                Token = response.RefreshToken,
                UserId = response.UserId,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
            }
        );

        await _context.SaveChangesAsync();
    }

    public override Task RefreshRequestValidationAsync(TokenRequest req)
    {
        var refreshToken = _context.RefreshTokens.SingleOrDefault(rt =>
            rt.Token == req.RefreshToken
        );

        if (refreshToken is null)
        {
            AddError("Invalid refresh token", nameof(HttpStatusCode.Unauthorized));
            return Task.CompletedTask;
        }

        if (refreshToken.ExpiresAt < DateTimeOffset.UtcNow)
        {
            AddError("Refresh token has expired", nameof(HttpStatusCode.Unauthorized));
            return Task.CompletedTask;
        }

        if (refreshToken.IsRevoked)
        {
            AddError("Refresh token has been revoked", nameof(HttpStatusCode.Unauthorized));
            return Task.CompletedTask;
        }

        if (refreshToken.UserId != req.UserId)
        {
            AddError(
                "Refresh token does not belong to the specified user",
                nameof(HttpStatusCode.Unauthorized)
            );
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }

    public override Task SetRenewalPrivilegesAsync(TokenRequest request, UserPrivileges privileges)
    {
        var user = _context
            .Users.AsNoTracking()
            .Include(u => u.Roles)
            .Single(u => u.Id == request.UserId);

        privileges.Claims.AddRange([
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.Username),
        ]);
        privileges.Roles.AddRange(user.Roles.Select(r => r.Name));

        return Task.CompletedTask;
    }
}
