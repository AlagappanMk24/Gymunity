using Gymunity.Application.DTOs.User.Payment;
using Gymunity.Domain.Enums;

namespace Gymunity.Admin.MVC.ViewModels.Payments
{
    public class PaymentsListViewModel
    {
        public List<PaymentResponse> Payments { get; set; } = new List<PaymentResponse>();
        
        public int PageNumber { get; set; } = 1;
        
        public int PageSize { get; set; } = 10;
        
        public int TotalCount { get; set; } = 0;
        
        public PaymentStatus? StatusFilter { get; set; }
        
        public DateTime? StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Total number of pages
        /// </summary>
        public int TotalPages => (TotalCount + PageSize - 1) / PageSize;

        /// <summary>
        /// Check if there are more pages
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;

        /// <summary>
        /// Check if there are previous pages
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;

        /// <summary>
        /// Get total revenue from displayed payments
        /// </summary>
        public decimal TotalRevenue => Payments.Sum(p => p.Amount);
    }
}
