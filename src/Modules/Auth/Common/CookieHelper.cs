using FastEndpoints.Security;
using Microsoft.AspNetCore.Http;

namespace Daab.Modules.Auth.Common;

public static class CookieHelper
{
    extension(HttpContext httpContext)
    {
        public void SetAuthCookies(TokenResponse response)
        {
            httpContext.Response.Cookies.Append(
                "daab.accessToken",
                response.AccessToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(15),
                    SameSite = SameSiteMode.Lax,
                }
            );
            httpContext.Response.Cookies.Append(
                "daab.refreshToken",
                response.RefreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.UtcNow.AddDays(7),
                    SameSite = SameSiteMode.Lax,
                }
            );
            httpContext.Response.Cookies.Append(
                "daab.userId",
                response.UserId,
                new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.UtcNow.AddDays(7),
                    SameSite = SameSiteMode.Lax,
                }
            );
        }
    }
}
