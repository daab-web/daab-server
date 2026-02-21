using System.Security.Cryptography;
using Daab.Modules.Auth.Models;

namespace Daab.Modules.Auth.Common;

public static class TokenService
{
    public static RefreshToken GenerateRefreshToken(string userId)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        return new RefreshToken { Token = token, UserId = userId };
    }
}
