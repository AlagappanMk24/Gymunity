using Gymunity.Domain.Entities;
using Gymunity.Domain.Specification;
using System.Linq.Expressions;

public class PaymentByGatewayOrderSpecs
    : BaseSpecification<Payment>
{
    private PaymentByGatewayOrderSpecs(
        Expression<Func<Payment, bool>> criteria)
        : base(criteria)
    {
        AddInclude(p => p.Subscription);
    }

    public static PaymentByGatewayOrderSpecs ForPayPal(string orderId)
    {
        return new PaymentByGatewayOrderSpecs(
            p => p.PayPalOrderId == orderId && !p.IsDeleted);
    }

    public static PaymentByGatewayOrderSpecs ForPaymob(string orderId)
    {
        return new PaymentByGatewayOrderSpecs(
            p => p.PaymobOrderId == orderId && !p.IsDeleted);
    }
}
