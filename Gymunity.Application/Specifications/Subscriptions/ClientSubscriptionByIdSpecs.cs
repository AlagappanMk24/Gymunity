using Gymunity.Domain.Entities;
using Gymunity.Domain.Specification;
using Microsoft.EntityFrameworkCore;

namespace ITI.Gymunity.FP.Application.Specefications.Subscriptions
{
    /// <summary>
    /// Get single subscription by ID for a specific client
    /// Includes all related data for complete context
    /// </summary>
    public class ClientSubscriptionByIdSpecs : BaseSpecification<Subscription>
    {
        public ClientSubscriptionByIdSpecs(int id, string clientId)
        : base(s => s.Id == id && s.ClientId == clientId)
        {
            // ✅ Include client for response mapping
            AddInclude(s => s.Client);
            
            // ✅ Include package with trainer for payment context
            AddInclude(query => query
                .Include(s => s.Package)
                    .ThenInclude(p => p.Trainer)
                        .ThenInclude(t => t.User));
        }
    }
}