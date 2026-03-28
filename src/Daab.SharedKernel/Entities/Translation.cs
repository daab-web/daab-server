using Microsoft.EntityFrameworkCore;

namespace Daab.SharedKernel;

public class Translation
{
    public string Id { get; } = Ulid.NewUlid().ToString();
    public required string EntityType { get; init; }
    public required string EntityId { get; init; }
    public required string Locale { get; init; }
    public required string Field { get; init; }
    public required string Value { get; set; }
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public void Update(string value)
    {
        Value = value;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

public class TranslationsManager<TEntity>(DbContext ctx)
{
    private readonly DbSet<Translation> _translations = ctx.Set<Translation>();
    private readonly string _entityType = typeof(TEntity).Name;

    public async Task<Dictionary<string, string>> GetAsync(
        string entityId,
        string locale,
        string fallbackLocale = "en",
        CancellationToken ct = default
    )
    {
        var rows = await _translations
            .Where(t =>
                t.EntityType == _entityType
                && t.EntityId == entityId
                && (t.Locale == locale || t.Locale == fallbackLocale)
            )
            .ToListAsync(ct);

        return rows.GroupBy(t => t.Field)
            .ToDictionary(
                g => g.Key,
                g => g.FirstOrDefault(t => t.Locale == locale)?.Value ?? g.First().Value
            );
    }

    public async Task<List<Translation>> GetAllLocalesAsync(
        string entityId,
        CancellationToken ct = default
    )
    {
        return await _translations
            .Where(t => t.EntityType == _entityType && t.EntityId == entityId)
            .ToListAsync(ct);
    }

    public async Task SetAsync(
        string entityId,
        string locale,
        string field,
        string value,
        CancellationToken ct = default
    )
    {
        var existing = await _translations.FirstOrDefaultAsync(
            t =>
                t.EntityType == _entityType
                && t.EntityId == entityId
                && t.Locale == locale
                && t.Field == field,
            ct
        );

        if (existing is null)
        {
            await _translations.AddAsync(
                new Translation
                {
                    EntityType = _entityType,
                    EntityId = entityId,
                    Locale = locale,
                    Field = field,
                    Value = value,
                },
                ct
            );
        }
        else
        {
            existing.Update(value);
        }
    }

    public async Task SetManyAsync(
        string entityId,
        string locale,
        Dictionary<string, string> fields,
        CancellationToken ct = default
    )
    {
        var existing = await _translations
            .Where(t =>
                t.EntityType == _entityType
                && t.EntityId == entityId
                && t.Locale == locale
                && fields.Keys.Contains(t.Field)
            )
            .ToListAsync(ct);

        foreach ((string field, string value) in fields)
        {
            var translation = existing.FirstOrDefault(t => t.Field == field);

            if (translation is null)
            {
                await _translations.AddAsync(
                    new Translation
                    {
                        EntityType = _entityType,
                        EntityId = entityId,
                        Locale = locale,
                        Field = field,
                        Value = value,
                    },
                    ct
                );
            }
            else
            {
                translation.Update(value);
            }
        }
    }

    public async Task<int> DeleteAsync(string id, CancellationToken ct = default)
    {
        return await _translations.Where(t => t.Id == id).ExecuteDeleteAsync(ct);
    }

    public async Task<int> DeleteAllForEntityAsync(string entityId, CancellationToken ct = default)
    {
        return await _translations
            .Where(t => t.EntityType == _entityType && t.EntityId == entityId)
            .ExecuteDeleteAsync(ct);
    }

    public async Task<int> DeleteLocaleForEntityAsync(
        string entityId,
        string locale,
        CancellationToken ct = default
    )
    {
        return await _translations
            .Where(t => t.EntityType == _entityType && t.EntityId == entityId && t.Locale == locale)
            .ExecuteDeleteAsync(ct);
    }
}
