using System.Text;
using Daab.Modules.Translation.Lexical;
using Daab.Modules.Translation.Memory;
using Daab.Modules.Translation.Models;
using GlossarySet = Daab.Modules.Translation.Glossary.Glossary;

namespace Daab.Modules.Translation.Service;

/// <summary>
/// Assembles the two-part prompt. The system instruction (role, rules, glossary) is stable across
/// a locale direction and content type — the natural Gemini context-cache prefix. The user content
/// (few-shot examples + numbered tasks) varies per document.
/// </summary>
internal static class PromptBuilder
{
    public static (string System, string User) Build(
        ContentType contentType,
        string sourceLocale,
        string targetLocale,
        GlossarySet glossary,
        IReadOnlyList<TranslationExample> examples,
        IReadOnlyList<LexicalBlock> tasks
    )
    {
        var system = $"""
            You are a professional translator for a national academy of sciences.
            Translate {LocaleName(sourceLocale)} source text into {LocaleName(targetLocale)}.

            Rules:
            - Keep a formal, encyclopedic register and preserve the source meaning.
            - Preserve every inline tag such as <0>...</0> exactly: identical numbers and nesting,
              never add, drop, or renumber a tag. Translate only the text between tags.
            - Do not translate proper names or institution labels; render any glossary term exactly
              as its target.
            - Respond with a JSON array of strings only. No prose, no code fences.

            Content type: {ContentTypeName(contentType)}
            Glossary (render each source exactly as its target):
            {(glossary.IsEmpty ? "(none)" : glossary.Render())}
            """;

        var user = new StringBuilder();
        if (examples.Count > 0)
        {
            user.AppendLine("Reference translations (match their style and terminology):");
            for (var i = 0; i < examples.Count; i++)
            {
                user.AppendLine($"{i + 1}. SOURCE: {examples[i].Source}");
                user.AppendLine($"   TARGET: {examples[i].Target}");
            }

            user.AppendLine();
        }

        user.AppendLine(
            $"Translate these {tasks.Count} items into {LocaleName(targetLocale)}. Return a JSON "
                + $"array of exactly {tasks.Count} strings, where element i is the translation of "
                + "item i in the same order. Preserve all tags."
        );
        user.AppendLine();
        for (var i = 0; i < tasks.Count; i++)
        {
            user.AppendLine($"{i + 1}. {tasks[i].Source}");
        }

        return (system, user.ToString());
    }

    static string LocaleName(string code) =>
        code switch
        {
            "en" => "English",
            "az" => "Azerbaijani",
            "ru" => "Russian",
            _ => code,
        };

    static string ContentTypeName(ContentType contentType) =>
        contentType switch
        {
            ContentType.Scientist => "scientist biography",
            ContentType.Article => "news article",
            _ => contentType.ToString(),
        };
}
