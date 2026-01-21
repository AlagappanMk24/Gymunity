using Gymunity.Domain.Entities;
using Gymunity.Domain.Interfaces;

namespace Gymunity.Domain
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        IRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity;
        TRepo Repository<TEntity, TRepo>()
            where TRepo : IRepository<TEntity>
            where TEntity : BaseEntity;
        Task<int> CompleteAsync();
    }
}