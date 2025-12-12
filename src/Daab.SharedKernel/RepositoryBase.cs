using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Daab.SharedKernel;

public class RepositoryBase<TEntity>(DbContext context) : IRepository<TEntity>
    where TEntity : class
{
    protected DbSet<TEntity> Set = context.Set<TEntity>();

    public virtual async Task<IEnumerable<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = Set.AsNoTracking().AsQueryable();
        if (filter is not null)
        {
            query = query.Where(filter);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public virtual async ValueTask<TEntity?> FindAsync(
        object id,
        CancellationToken cancellationToken = default
    )
    {
        return await Set.FindAsync([id], cancellationToken: cancellationToken);
    }

    public virtual async ValueTask InsertAsync(
        TEntity entity,
        CancellationToken cancellationToken = default
    )
    {
        await Set.AddAsync(entity, cancellationToken);
    }

    public virtual async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}

