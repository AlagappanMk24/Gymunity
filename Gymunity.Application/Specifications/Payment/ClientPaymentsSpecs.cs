using Gymunity.Domain.Entities;
using Gymunity.Domain.Enums;
using Gymunity.Domain.Specification;
using Microsoft.EntityFrameworkCore;
using DomainPayment = Gymunity.Domain.Entities.Payment;

namespace Gymunity.Application.Specifications.Payment
{
    public class ClientPaymentsSpecs : BaseSpecification<DomainPayment>
    {
        public ClientPaymentsSpecs(string clientId, PaymentStatus? status = null)
            : base(p => p.ClientId == clientId 
                     && !p.IsDeleted
                     && (!status.HasValue || p.Status == status.Value))
        {
            // Include relations for display
            AddInclude(query => query
                .Include(p => p.Subscription)
                .ThenInclude(s => s.Package));

            AddOrderByDesc(p => p.CreatedAt);
        }
    }
}