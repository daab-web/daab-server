namespace Daab.Modules.Translation.Glossary;

/// <summary>
/// A neutral, storage-agnostic view of one ReferenceData translation row. The
/// concrete provider (Step 4, host side) fills these from
/// <c>ReferenceData.Translation</c> so the projection stays free of any
/// cross-module dependency and is unit-testable without a database.
/// </summary>
public sealed record GlossaryRow(string Namespace, string Key, string Locale, string Value);
