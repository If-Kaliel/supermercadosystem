using Microsoft.EntityFrameworkCore;
using Supermercado.Application.Repositories;
using Supermercado.Infrastructure.Persistence;

namespace Supermercado.Infrastructure.Repositories;

public class Repository<T>(SupermercadoContext context) : IRepository<T> where T : class
{
    public async Task<IReadOnlyList<T>> ListAsync(CancellationToken cancellationToken = default)
        => await context.Set<T>().AsNoTracking().ToListAsync(cancellationToken);

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Set<T>().FindAsync([id], cancellationToken);

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        => await context.Set<T>().AddAsync(entity, cancellationToken);

    public void Update(T entity) => context.Set<T>().Update(entity);

    public void Remove(T entity) => context.Set<T>().Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);
}
