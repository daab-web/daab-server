using Microsoft.AspNetCore.Http;

namespace Daab.Modules.Auth.Common;

public static class CookieHelper
{
    extension(HttpContext httpContext)
    {
        public void SetAuthCookies(string accessToken, string refreshToken, string userId)
        {
            httpContext.Response.Cookies.Append(
                "daab.accessToken",
                accessToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTime.UtcNow.AddMinutes(15),
                    SameSite = SameSiteMode.Lax,
                }
            );
            httpContext.Response.Cookies.Append(
                "daab.refreshToken",
                refreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTime.UtcNow.AddDays(7),
                    SameSite = SameSiteMode.Lax,
                }
            );
            httpContext.Response.Cookies.Append(
                "daab.userId",
                userId,
                new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTime.UtcNow.AddDays(7),
                    SameSite = SameSiteMode.Lax,
                }
            );
        }
    }
}
