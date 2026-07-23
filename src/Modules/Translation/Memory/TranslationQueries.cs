using Daab.Modules.Translation.Lexical;
using Daab.Modules.Translation.Models;

namespace Daab.Modules.Translation.Memory;

/// <summary>Which retrieval tier produced an example.</summary>
public enum ExampleTier
{
    Fuzzy,
    Semantic,
}

/// <summary>A retrieved few-shot pair (tagged source → tagged target) with its ranking score.</summary>
public sealed record TranslationExample(
    string Source,
    string Target,
    double Score,
    ExampleTier Tier
);

/// <summary>Key for the exact-hash tier: identifies a block up to its translation context.</summary>
public sealed record TranslationLookup(
    string SourceText,
    ContentType ContentType,
    BlockType BlockType,
    string SourceLocale,
    string TargetLocale
);

/// <summary>A request for few-shot examples, with the tiering knobs.</summary>
public sealed record TranslationQuery(
    string SourceText,
    ContentType ContentType,
    BlockType BlockType,
    string SourceLocale,
    string TargetLocale,
    int K = 5,
    double FuzzyThreshold = 0.8,
    double MmrLambda = 0.7,
    int MaxFuzzy = 2
);
