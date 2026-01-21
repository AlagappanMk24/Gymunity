using Gymunity.Domain.Entities.Trainer;
using Gymunity.Domain.Specification;
using Microsoft.EntityFrameworkCore;

namespace Gymunity.Application.Specifications.Admin
{
    /// <summary>
    /// Specification for filtering and retrieving trainer profiles with pagination
    /// Supports filtering by verification status, suspension status and sorting
    /// </summary>
    public class TrainerFilterSpecs : BaseSpecification<TrainerProfile>
    {
        public TrainerFilterSpecs(
            bool? isVerified = null,
            bool? isSuspended = null,
            int pageNumber = 1,
            int pageSize = 10,
            string? searchTerm = null) 
            : base(tp => (!isVerified.HasValue || isVerified.Value == tp.IsVerified)
                      && (!isSuspended.HasValue || isSuspended.Value == tp.IsSuspended)
                      && (string.IsNullOrEmpty(searchTerm) 
                        || EF.Functions.Like(tp.Handle, $"%{searchTerm}%")
                        || EF.Functions.Like(tp.User.FullName, $"%{searchTerm}%")
                        || EF.Functions.Like(tp.User.Email, $"%{searchTerm}%")
                        || EF.Functions.Like(tp.User.UserName, $"%{searchTerm}%")))
        {

            // Eager load related data
            AddInclude(t => t.User);

            // Sort by newest first
            AddOrderByDesc(t => t.CreatedAt);

            // Apply pagination
            ApplyPagination((pageNumber - 1) * pageSize, pageSize);
        }
    }
}