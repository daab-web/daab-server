using System.Linq.Expressions;

namespace Daab.SharedKernel;

public interface IRepository<TEntity>
{
    Task<IEnumerable<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        CancellationToken cancellationToken = default
    );
    ValueTask InsertAsync(TEntity entity, CancellationToken cancellationToken = default);
    ValueTask<TEntity?> FindAsync(object id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

