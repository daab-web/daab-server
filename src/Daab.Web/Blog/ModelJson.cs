namespace Daab.Web.Blog;

internal static class ModelJson
{
    /// <summary>Strips a Markdown code fence the model sometimes wraps JSON in.</summary>
    public static string Unwrap(string raw)
    {
        var text = raw.Trim();
        if (!text.StartsWith("```", StringComparison.Ordinal))
        {
            return text;
        }

        var firstNewLine = text.IndexOf('\n');
        var lastFence = text.LastIndexOf("```", StringComparison.Ordinal);

        return firstNewLine >= 0 && lastFence > firstNewLine
            ? text[(firstNewLine + 1)..lastFence].Trim()
            : text.Trim('`');
    }
}

internal static class LanguageNames
{
    public static string Of(string locale) =>
        locale.ToLowerInvariant() switch
        {
            "en" => "English",
            "az" => "Azerbaijani",
            "ru" => "Russian",
            "tr" => "Turkish",
            _ => locale,
        };
}
