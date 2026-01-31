using Gymunity.Domain.Entities;
using Gymunity.Domain.Specification;
using Microsoft.EntityFrameworkCore;
using DomainPayment = Gymunity.Domain.Entities.Payment;

namespace Gymunity.Application.Specifications.Payment
{
    public class PaymentByIdSpecs : BaseSpecification<DomainPayment>
    {
        public PaymentByIdSpecs(int id, string clientId)
            : base(p => p.Id == id
                     && p.ClientId == clientId 
                     && !p.IsDeleted)
        {
            AddInclude(query => query
                .Include(p => p.Subscription)
                .ThenInclude(s => s.Package));
        }
    }
}