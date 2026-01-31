using Gymunity.Domain.Enums;
using Gymunity.Domain.Specification;
using DomainPayment = Gymunity.Domain.Entities.Payment;

namespace Gymunity.Application.Specifications.Payment
{
    public class PaymentBySubscriptionSpecs : BaseSpecification<DomainPayment>
    {
        public PaymentBySubscriptionSpecs(int subscriptionId)
             : base(p =>
                p.SubscriptionId == subscriptionId &&
                p.Status == PaymentStatus.Pending &&
                !p.IsDeleted)
        {
            AddInclude(p => p.Subscription);
            AddOrderByDesc(p => p.CreatedAt);
        }

        public PaymentBySubscriptionSpecs(int subscriptionId, string clientId)
             : base(p =>
                p.SubscriptionId == subscriptionId &&
                p.ClientId == clientId &&
                !p.IsDeleted)
        {
            AddInclude(p => p.Subscription);
            AddOrderByDesc(p => p.CreatedAt);
        }
    }
}
    