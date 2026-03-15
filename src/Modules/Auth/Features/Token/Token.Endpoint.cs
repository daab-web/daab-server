using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Daab.Modules.Auth.Common;
using Daab.Modules.Auth.Models;
using Daab.Modules.Auth.Persistence;
using FastEndpoints;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Daab.Modules.Auth.Features.Token;

public class TokenEndpoint(
    AuthDbContext ctx,
    ITokenService tokenService,
    IDataProtectionProvider dataProtectionProvider,
    ILogger<TokenEndpoint> logger
) : Endpoint<OAuthTokenRequest, OAuthTokenResponse>
{
    private readonly IDataProtector _dataProtector = dataProtectionProvider.CreateProtector(
        "oauth"
    );

    public override void Configure()
    {
        Post("/token");
        AllowAnonymous();
        AllowFormData(urlEncoded: true);
    }

    public override async Task HandleAsync(OAuthTokenRequest req, CancellationToken ct)
    {
        var codeString = _dataProtector.Unprotect(req.Code);
        var code = JsonSerializer.Deserialize<AuthCode>(codeString);

        if (code is null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        if (!ValidateCodeVerifier(code, req.CodeVerifier))
        {
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        var user = await ctx
            .Users.AsNoTracking()
            .Include(u => u.Roles)
            .SingleOrDefaultAsync(u => u.Id == code.UserId, ct);

        if (user is null)
        {
            await Send.NotFoundAsync(cancellation: ct);
            return;
        }

        var accessToken = tokenService.GenerateAccessToken(user);

        // TODO: Generate refresh tokens in service
        var refreshToken = Ulid.NewUlid().ToString().ToLower();

        logger.LogDebug("Responding with accessToken: {token}", accessToken);

        await Send.OkAsync(
            new OAuthTokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                TokenType = "Bearer",
            },
            ct
        );
    }

    private static bool ValidateCodeVerifier(AuthCode code, string codeVerifier)
    {
        string codeChallenge = Base64UrlEncoder.Encode(
            SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier))
        );

        return code.CodeChallenge == codeChallenge;
    }
}
