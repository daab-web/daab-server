using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using System.Web;
using Daab.Modules.Auth.Models;
using FastEndpoints;
using Microsoft.AspNetCore.DataProtection;

namespace Daab.Modules.Auth.Features.Authorize;

public class AuthorizeEndpoint(IDataProtectionProvider dataProtectionProvider)
    : Endpoint<AuthorizeRequest>
{
    private readonly IDataProtector _dataProtector = dataProtectionProvider.CreateProtector(
        "oauth"
    );

    public override void Configure()
    {
        Get("/authorize");
        AuthSchemes("cookie");
    }

    public override async Task HandleAsync(AuthorizeRequest req, CancellationToken ct)
    {
        var code = new AuthCode
        {
            ClientId = req.ClientId,
            UserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)!,
            CodeChallenge = req.CodeChallenge,
            CodeChallengeMethod = req.CodeChallengeMethod,
            RedirectUri = req.RedirectUri,
            Expiry = DateTime.UtcNow.AddMinutes(5),
        };

        var codeString = _dataProtector.Protect(JsonSerializer.Serialize(code));

        await Send.RedirectAsync(
            $"{req.RedirectUri}?code={HttpUtility.UrlEncode(codeString)}&state={HttpUtility.UrlEncode(req.State)}&iss={HttpUtility.UrlEncode("http://localhost:5035")}",
            allowRemoteRedirects: true
        );
    }
}
