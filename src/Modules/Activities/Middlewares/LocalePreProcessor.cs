using Daab.SharedKernel.Options;
using FastEndpoints;
using Microsoft.Extensions.Options;

namespace Daab.Modules.Activities.Middlewares;

public interface ILocalized
{
    public string Locale { get; }
}

public class LocalePreProcessor(IOptionsMonitor<LocaleOptions> opts) : IPreProcessor<ILocalized>
{
    private readonly LocaleOptions _localeOptions = opts.CurrentValue;

    public Task PreProcessAsync(IPreProcessorContext<ILocalized> context, CancellationToken ct)
    {
        if (context.Request is null)
        {
            return Task.CompletedTask;
        }

        if (!_localeOptions.SupportedLocales.Contains(context.Request.Locale))
        {
            context.ValidationFailures.Add(
                new("UnsupportedLocale", "Requested locale in not supported")
            );

            return context.HttpContext.Response.SendErrorsAsync(
                context.ValidationFailures,
                cancellation: ct
            );
        }

        return Task.CompletedTask;
    }
}
