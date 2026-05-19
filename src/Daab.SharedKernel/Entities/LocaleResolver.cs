using Daab.SharedKernel.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Daab.SharedKernel.Entities;

public interface ILocaleResolver
{
    string Resolve();
}

public class LocaleResolver(
    IHttpContextAccessor httpContextAccessor,
    IOptionsMonitor<LocaleOptions> opts
) : ILocaleResolver
{
    private readonly LocaleOptions _options = opts.CurrentValue;
    public const string Fallback = "en";

    public string Resolve()
    {
        HttpContext context =
            httpContextAccessor.HttpContext
            ?? throw new NullReferenceException(nameof(httpContextAccessor));
        var acceptLanguage = context.Request.GetTypedHeaders().AcceptLanguage;

        if (acceptLanguage is null or { Count: 0 })
        {
            return "en";
        }

        return acceptLanguage
                .OrderByDescending(l => l.Quality ?? 1.0)
                .Select(l => l.Value.ToString())
                .SelectMany(v => new[] { v, v.Split('-')[0] })
                .FirstOrDefault(l => _options.SupportedLocales.Contains(l))
            ?? Fallback;
    }
}
