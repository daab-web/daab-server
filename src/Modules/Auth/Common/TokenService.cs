using System.Net;
using System.Security.Claims;
using Daab.Modules.Auth.Models;
using Daab.Modules.Auth.Options;
using Daab.Modules.Auth.Persistence;
using FastEndpoints;
using FastEndpoints.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Daab.Modules.Auth.Common;

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

    public override Task OnAfterRenewalTokenCreationAsync(
        TokenRequest? request,
        TokenResponse response
    )
    {
        HttpContext.SetAuthCookies(response);
        return Task.CompletedTask;
    }

    public override void OnBeforeHandle(TokenRequest req)
    {
        if (string.IsNullOrEmpty(req.RefreshToken))
        {
            var hasRefreshToken = HttpContext.Request.Cookies.TryGetValue(
                "daab.refreshToken",
                out var refreshToken
            );

            var hasUserId = HttpContext.Request.Cookies.TryGetValue("daab.userId", out var userId);

            if (!hasRefreshToken || string.IsNullOrEmpty(refreshToken))
            {
                AddError("Refresh token is required", nameof(HttpStatusCode.BadRequest));
                return;
            }

            if (!hasUserId || string.IsNullOrEmpty(userId))
            {
                AddError("User ID is required", nameof(HttpStatusCode.BadRequest));
                return;
            }

            req.RefreshToken = refreshToken;
            req.UserId = userId;
        }
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

        var err = refreshToken switch
        {
            null => "Invalid refresh token",
            { ExpiresAt: var exp } when exp < DateTimeOffset.UtcNow => "Refresh token has expired",
            { IsRevoked: true } => "Refresh token has been revoked",
            { UserId: var id } when id != req.UserId =>
                "Refresh token does not belong to the specified user",
            _ => null,
        };

        if (err is not null)
        {
            AddError(err, nameof(HttpStatusCode.Unauthorized));
        }

        return Task.CompletedTask;
    }

    public override Task SetRenewalPrivilegesAsync(TokenRequest request, UserPrivileges privileges)
    {
        var refreshToken = _context
            .RefreshTokens.AsNoTracking()
            .Single(rt => rt.Token == request.RefreshToken);

        var user = _context
            .Users.AsNoTracking()
            .Include(u => u.Roles)
            .Single(u => u.Id == refreshToken.UserId);

        privileges.Claims.AddRange([
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.Username),
        ]);
        privileges.Roles.AddRange(user.Roles.Select(r => r.Name));

        return Task.CompletedTask;
    }
}
