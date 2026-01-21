using Gymunity.Domain.Entities;
using Gymunity.Domain.Specification;

namespace Gymunity.Domain.Interfaces
{
    public interface IRepository<TEntity> where TEntity : BaseEntity
    {
        Task<TEntity?> GetWithSpecsAsync(ISpecification<TEntity> specs);
        Task<TEntity?> GetByIdAsync(int id);
        Task<TEntity?> GetByIdAsync(long id); //amr edit
        Task<IEnumerable<TEntity>> GetAllWithSpecsAsync(ISpecification<TEntity> specs);
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<int> GetCountWithspecsAsync(ISpecification<TEntity> specs);
        void Add(TEntity entity);
        void Update(TEntity entity);
        void Delete(TEntity entity);
    }
}