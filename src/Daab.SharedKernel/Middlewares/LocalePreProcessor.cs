using Daab.SharedKernel.Options;
using FastEndpoints;
using FluentValidation.Results;
using Microsoft.Extensions.Options;

namespace Daab.SharedKernel.Middlewares;

public interface ILocalized
{
    public string Locale { get; }
}

public interface ILocalizedCollection
{
    IEnumerable<string> GetLocales();
}

public class LocaleCollectionPreProcessor(IOptionsMonitor<LocaleOptions> opts)
    : IPreProcessor<ILocalizedCollection>
{
    private readonly LocaleOptions _localeOptions = opts.CurrentValue;

    public Task PreProcessAsync(
        IPreProcessorContext<ILocalizedCollection> context,
        CancellationToken ct
    )
    {
        if (context.Request is null)
            return Task.CompletedTask;

        var unsupported = context
            .Request.GetLocales()
            .Where(l => !_localeOptions.SupportedLocales.Contains(l))
            .ToList();

        if (unsupported.Count == 0)
            return Task.CompletedTask;

        foreach (var locale in unsupported)
        {
            context.ValidationFailures.Add(
                new ValidationFailure("Locale", $"Locale '{locale}' is not supported")
            );
        }

        return context.HttpContext.Response.SendErrorsAsync(
            context.ValidationFailures,
            cancellation: ct
        );
    }
}

[Obsolete("Use LocalePreProcessor instead")]
public class LocalePreProcessor(IOptionsMonitor<LocaleOptions> opts) : IPreProcessor<ILocalized>
{
    private readonly LocaleOptions _localeOptions = opts.CurrentValue;

    public Task PreProcessAsync(IPreProcessorContext<ILocalized> context, CancellationToken ct)
    {
        if (
            context.Request is null
            || _localeOptions.SupportedLocales.Contains(context.Request.Locale)
        )
        {
            return Task.CompletedTask;
        }

        context.ValidationFailures.Add(
            new ValidationFailure("UnsupportedLocale", "Requested locale in not supported")
        );

        return context.HttpContext.Response.SendErrorsAsync(
            context.ValidationFailures,
            cancellation: ct
        );
    }
}
