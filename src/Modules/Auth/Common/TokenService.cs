using System.Text;
using Daab.Modules.Auth.Models;
using Daab.Modules.Auth.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Daab.Modules.Auth.Common;

public interface ITokenService
{
    string GenerateAccessToken(User user);
}

public class TokenService(IOptions<JwtOptions> options) : ITokenService
{
    private readonly JwtOptions _options = options.Value;

    public string GenerateAccessToken(User user)
    {
        var handler = new JsonWebTokenHandler();

        return handler.CreateToken(
            new SecurityTokenDescriptor
            {
                Claims = new Dictionary<string, object>
                {
                    [JwtRegisteredClaimNames.Sub] = user.Id,
                    [JwtRegisteredClaimNames.Name] = user.Username,
                    [JwtRegisteredClaimNames.Email] = "admin@waas.org",
                    ["roles"] = string.Join(',', user.Roles.Select(r => r.Name)),
                },
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
