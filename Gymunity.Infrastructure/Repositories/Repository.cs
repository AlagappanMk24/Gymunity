using Gymunity.Domain.Entities;
using Gymunity.Domain.Interfaces;
using Gymunity.Domain.Specification;
using Gymunity.Infrastructure.Data.Context;
using Gymunity.Infrastructure.Specification;
using Microsoft.EntityFrameworkCore;

namespace Gymunity.Infrastructure.Repositories
{
    public class Repository<T>(AppDbContext context) : IRepository<T> where T : BaseEntity
    {
        private protected readonly AppDbContext _context = context;
        public void Add(T entity) => _context.Add(entity);
        public void Update(T entity) => _context.Update(entity);
        public void Delete(T entity) => _context.Remove(entity);
        public async Task<IEnumerable<T>> GetAllWithSpecsAsync(ISpecification<T> specs)
            => await ApplySpecifications(specs).ToListAsync();
        public async Task<int> GetCountWithspecsAsync(ISpecification<T> specs)
            => await ApplySpecifications(specs).CountAsync();
        public async Task<T?> GetWithSpecsAsync(ISpecification<T> specs)
            => await ApplySpecifications(specs).FirstOrDefaultAsync();
        public async Task<T?> GetByIdAsync(int id)
            => await _context.Set<T>().FindAsync(id);

        //start amr edit
        // New long overload to support entities with long primary keys (e.g., Message)
        public async Task<T?> GetByIdAsync(long id)
            => await _context.Set<T>().FindAsync(id);
        //end amr edit
        public async Task<IEnumerable<T>> GetAllAsync()
            => await _context.Set<T>().ToListAsync();
        protected IQueryable<T> ApplySpecifications(ISpecification<T> specs)
            => SpecificationEvaluator<T>.BuildQuery(_context.Set<T>(), specs);
    }
}