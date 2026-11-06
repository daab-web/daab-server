using System.Text;
using Daab.Modules.Auth.Models;
using Daab.Modules.Auth.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Daab.Modules.Auth.Common;

public interface ITokenService
{
    string GenerateAccessToken(User user, AccountLink? link = null);
}

public class TokenService(IOptions<JwtOptions> options) : ITokenService
{
    private readonly JwtOptions _options = options.Value;

    public string GenerateAccessToken(User user, AccountLink? link = null)
    {
        link ??= AccountLink.None;
        var handler = new JsonWebTokenHandler();

        var roles = user
            .Roles.Select(r => r.Name)
            .Concat(link.Roles)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var claims = new Dictionary<string, object>
        {
            [JwtRegisteredClaimNames.Sub] = user.Id,
            [JwtRegisteredClaimNames.Name] = user.DisplayName,
            [JwtRegisteredClaimNames.PreferredUsername] = user.Username,
            [AuthClaims.Roles] = roles,
        };

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            claims[JwtRegisteredClaimNames.Email] = user.Email;
        }

        foreach (var (key, value) in link.Claims)
        {
            claims[key] = value;
        }

        return handler.CreateToken(
            new SecurityTokenDescriptor
            {
                Claims = claims,
                IssuedAt = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(_options.AccessExpiryMinutes),
                TokenType = "Bearer",
                Issuer = _options.Issuer,
                Audience = _options.Audience,
                // TODO: Change this
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.JwtSecret)),
                    SecurityAlgorithms.HmacSha256
                ),
            }
        );
    }
}
