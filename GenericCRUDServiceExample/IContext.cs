using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace GenericCRUDServiceExample;

public interface IContext : IAsyncDisposable, IDisposable
{
    public DatabaseFacade Database { get; }
    public DbSet<TEntity> Set<TEntity>() where TEntity : class;
    public EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
    public DbSet<Hero> Heroes { get; }
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}