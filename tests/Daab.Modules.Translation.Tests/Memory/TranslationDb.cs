using Daab.Modules.Translation.Models;
using Daab.Modules.Translation.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Daab.Modules.Translation.Tests.Memory;

/// <summary>
/// A real SQLite <c>:memory:</c> database (connection kept open for its lifetime) so tests exercise
/// the actual BLOB value converter and SQL filter translation, not the in-memory provider.
/// </summary>
internal sealed class TranslationDb : IDisposable
{
    readonly SqliteConnection connection;

    public TranslationDbContext Context { get; }

    public TranslationDb()
    {
        connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        Context = new TranslationDbContext(
            new DbContextOptionsBuilder<TranslationDbContext>().UseSqlite(connection).Options
        );
        Context.Database.EnsureCreated();
    }

    /// <summary>Persists entries, then clears the tracker so later queries materialize from the DB.</summary>
    public void Seed(params TranslationMemoryEntry[] entries)
    {
        Context.Entries.AddRange(entries);
        Context.SaveChanges();
        Context.ChangeTracker.Clear();
    }

    public void Dispose()
    {
        Context.Dispose();
        connection.Dispose();
    }
}
