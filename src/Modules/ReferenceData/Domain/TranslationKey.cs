using System.Text.RegularExpressions;

namespace Daab.Modules.ReferenceData.Domain;

public static partial class TranslationKey
{
    // Mirrors the client rule `nameEn.replace(/\W+/g, "")`. ECMAScript option keeps
    // \W as [^A-Za-z0-9_] so derived keys stay identical to existing scientist records.
    [GeneratedRegex(@"\W+", RegexOptions.ECMAScript)]
    private static partial Regex NonWord();

    public static string From(string nameEn) => NonWord().Replace(nameEn, "");
}
