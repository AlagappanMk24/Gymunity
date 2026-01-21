using Gymunity.Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Gymunity.Domain.Specification
{
    public interface ISpecification<T> where T : BaseEntity
    {
        Expression<Func<T, bool>>? Criteria { get; }
        List<Expression<Func<T, object>>> Includes { get; }
        List<Func<IQueryable<T>, IIncludableQueryable<T, object>>> IncludeFuncs { get; }
        Expression<Func<T, object>> OrderByAsc { get; }
        Expression<Func<T, object>> OrderByDesc { get; }
        int Skip { get; }
        int Take { get; }
        bool IsPagenationEnabled { get; }
    }
}