using Gymunity.Admin.MVC.Controllers;
using Gymunity.Admin.MVC.ViewModels.Payments;
using Gymunity.Application.Contracts.Services.Admin;
using Gymunity.Application.Services.Admin;
using Gymunity.Application.Specifications.Admin;
using Gymunity.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITI.Gymunity.FP.Admin.MVC.Controllers
{
    /// <summary>
    /// Admin Payments Controller
    /// Manages payment transactions, refunds, and payment analytics
    /// </summary>
    [Authorize(Roles = "Admin")]
    [Route("admin")]
    public class PaymentsController(
        ILogger<PaymentsController> logger,
        IPaymentAdminService paymentService) : BaseAdminController
    {
        private readonly ILogger<PaymentsController> _logger = logger;
        private readonly IPaymentAdminService _paymentService = paymentService;

        /// <summary>
        /// Displays list of all payments with filtering and pagination
        /// </summary>
        [HttpGet("payments")]
        public async Task<IActionResult> Index(
            int pageNumber = 1,
            int pageSize = 10,
            PaymentStatus? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                SetPageTitle("Manage Payments");
                SetBreadcrumbs("Payments");

                var specs = new PaymentFilterSpecs(
                    status: status,
                    startDate: startDate,
                    endDate: endDate,
                    pageNumber: pageNumber,
                    pageSize: pageSize);

                var payments = await _paymentService.GetAllPaymentsAsync(specs);
                var totalCount = await _paymentService.GetPaymentCountAsync(specs);

                var model = new PaymentsListViewModel
                {
                    Payments = payments.ToList(),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    StatusFilter = status,
                    StartDate = startDate,
                    EndDate = endDate
                };

                _logger.LogInformation("Payments list accessed by user: {User}", User.Identity?.Name);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading payments list");
                ShowErrorMessage("An error occurred while loading payments");
                return RedirectToDashboard();
            }
        }

        /// <summary>
        /// Displays detailed view of a specific payment
        /// </summary>
        [HttpGet("payments/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                SetPageTitle("Payment Details");
                SetBreadcrumbs("Dashboard", "Payments", "Details");

                // Use the method that loads all client and subscription information with the specification
                var payment = await _paymentService.GetPaymentDetailsWithClientAsync(id);
                if (payment == null)
                {
                    ShowErrorMessage("Payment not found");
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogInformation("Payment {PaymentId} details viewed by {User}", id, User.Identity?.Name);
                return View(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading payment details for ID {PaymentId}", id);
                ShowErrorMessage("An error occurred while loading payment details");
                return RedirectToDashboard();
            }
        }

        /// <summary>
        /// Displays failed payments
        /// </summary>
        [HttpGet("payments/failed")]
        public async Task<IActionResult> Failed(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                SetPageTitle("Failed Payments");
                SetBreadcrumbs("Dashboard", "Payments", "Failed");

                var payments = await _paymentService.GetFailedPaymentsAsync(pageNumber, pageSize);
                var totalCount = await _paymentService.GetPaymentCountAsync(
                    new PaymentFilterSpecs(status: PaymentStatus.Failed));

                var model = new PaymentsListViewModel
                {
                    Payments = payments.ToList(),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    StatusFilter = PaymentStatus.Failed
                };

                _logger.LogInformation("Failed payments list accessed by {User}", User.Identity?.Name);
                return View("Index", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading failed payments");
                ShowErrorMessage("An error occurred while loading failed payments");
                return RedirectToDashboard();
            }
        }

        /// <summary>
        /// Displays completed payments
        /// </summary>
        [HttpGet("payments/completed")]
        public async Task<IActionResult> Completed(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                SetPageTitle("Completed Payments");
                SetBreadcrumbs("Dashboard", "Payments", "Completed");

                var payments = await _paymentService.GetCompletedPaymentsAsync(pageNumber, pageSize);
                var totalCount = await _paymentService.GetPaymentCountAsync(
                    new PaymentFilterSpecs(status: PaymentStatus.Completed));

                var model = new PaymentsListViewModel
                {
                    Payments = payments.ToList(),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    StatusFilter = PaymentStatus.Completed
                };

                _logger.LogInformation("Completed payments list accessed by {User}", User.Identity?.Name);
                return View("Index", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading completed payments");
                ShowErrorMessage("An error occurred while loading completed payments");
                return RedirectToDashboard();
            }
        }

        /// <summary>
        /// Processes a refund for a payment
        /// </summary>
        [HttpPost("payments/{id}/refund")]
        public async Task<IActionResult> Refund(int id)
        {
            try
            {
                var result = await _paymentService.ProcessRefundAsync(id);
                if (!result)
                    return BadRequest(new { success = false, message = "Failed to process refund" });

                ShowSuccessMessage("Refund processed successfully");
                _logger.LogInformation("Payment {PaymentId} refunded by {User}", id, User.Identity?.Name);
                return Ok(new { success = true, message = "Refund processed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund for payment {PaymentId}", id);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Gets total revenue for a date range
        /// </summary>
        [HttpGet("payments/revenue")]
        public async Task<IActionResult> GetRevenue(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                if (!startDate.HasValue || !endDate.HasValue)
                {
                    return BadRequest(new { success = false, message = "Both start and end dates are required" });
                }

                var revenue = await _paymentService.GetRevenueAsync(startDate.Value, endDate.Value);

                return Ok(new { success = true, revenue = revenue });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating revenue");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Gets total revenue (all completed payments)
        /// </summary>
        [HttpGet("payments/total-revenue")]
        public async Task<IActionResult> GetTotalRevenue()
        {
            try
            {
                var totalRevenue = await _paymentService.GetTotalRevenueAsync();
                return Ok(new { success = true, totalRevenue = totalRevenue });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total revenue");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Gets count of failed payments
        /// </summary>
        [HttpGet("payments/failed-count")]
        public async Task<IActionResult> GetFailedPaymentCount()
        {
            try
            {
                var count = await _paymentService.GetFailedPaymentCountAsync();
                return Ok(new { success = true, count = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting failed payment count");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// AJAX endpoint for DataTable - Returns payments as JSON
        /// </summary>
        [HttpGet("payments/list-json")]
        public async Task<IActionResult> GetPaymentsJson(
            int draw = 1,
            int start = 0,
            int length = 10,
            string? status = null)
        {
            try
            {
                var pageNumber = (start / length) + 1;
                PaymentStatus? paymentStatus = null;

                if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PaymentStatus>(status, out var parsedStatus))
                {
                    paymentStatus = parsedStatus;
                }

                var specs = new PaymentFilterSpecs(
                    status: paymentStatus,
                    pageNumber: pageNumber,
                    pageSize: length);

                var payments = await _paymentService.GetAllPaymentsAsync(specs);
                var totalCount = await _paymentService.GetPaymentCountAsync(new PaymentFilterSpecs());

                return Json(new
                {
                    draw = draw,
                    recordsTotal = totalCount,
                    recordsFiltered = totalCount,
                    data = payments
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payments JSON");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// AJAX endpoint for advanced payment filtering with multiple criteria
        /// Supports status, client, trainer, amount range, and date range filters
        /// </summary>
        [HttpPost("payments/filter")]
        public async Task<IActionResult> FilterPayments([FromBody] PaymentFilterRequest filterRequest)
        {
            try
            {
                if (filterRequest == null)
                {
                    return BadRequest(new { success = false, message = "Invalid filter request" });
                }

                // Get filtered payments using specification
                var payments = await _paymentService.GetPaymentsWithAdvancedFilterAsync(
                    status: filterRequest.Status,
                    clientSearch: filterRequest.ClientSearch,
                    trainerProfileId: filterRequest.TrainerProfileId,
                    minAmount: filterRequest.MinAmount,
                    maxAmount: filterRequest.MaxAmount,
                    startDate: filterRequest.StartDate,
                    endDate: filterRequest.EndDate,
                    pageNumber: filterRequest.PageNumber,
                    pageSize: filterRequest.PageSize);

                // Get total count for pagination
                var totalCount = await _paymentService.GetPaymentCountWithAdvancedFilterAsync(
                    status: filterRequest.Status,
                    clientSearch: filterRequest.ClientSearch,
                    trainerProfileId: filterRequest.TrainerProfileId,
                    minAmount: filterRequest.MinAmount,
                    maxAmount: filterRequest.MaxAmount,
                    startDate: filterRequest.StartDate,
                    endDate: filterRequest.EndDate);

                int totalPages = (int)Math.Ceiling((decimal)totalCount / filterRequest.PageSize);

                var filterResponse = new
                {
                    success = true,
                    data = new
                    {
                        payments = payments.Select(p => new
                        {
                            id = p.Id,
                            clientName = p.ClientName,
                            clientEmail = p.ClientEmail,
                            packageName = p.PackageName,
                            trainerName = p.TrainerName,
                            amount = p.Amount,
                            currency = p.Currency,
                            status = p.Status.ToString(),
                            method = p.Method.ToString(),
                            createdAt = p.CreatedAt.ToString("MMM dd, yyyy HH:mm"),
                            paidAt = p.PaidAt?.ToString("MMM dd, yyyy HH:mm"),
                            failureReason = p.FailureReason
                        }),
                        totalCount = totalCount,
                        pageNumber = filterRequest.PageNumber,
                        pageSize = filterRequest.PageSize,
                        hasNextPage = filterRequest.PageNumber < totalPages,
                        hasPreviousPage = filterRequest.PageNumber > 1,
                        totalPages = totalPages,
                        totalRevenue = payments.Sum(p => p.Amount)
                    }
                };

                _logger.LogDebug("Applied payment filters: Status={Status}, Client={Client}, Trainer={Trainer}",
                    filterRequest.Status, filterRequest.ClientSearch, filterRequest.TrainerProfileId);

                return Json(filterResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering payments");
                return BadRequest(new
                {
                    success = false,
                    message = "Error applying filters",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// AJAX endpoint for getting payment statistics
        /// </summary>
        [HttpGet("payments/stats")]
        public async Task<IActionResult> GetPaymentStats()
        {
            try
            {
                var (totalPayments, totalRevenue, completedCount, failedCount) =
                    await _paymentService.GetPaymentStatsAsync();

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        totalPayments,
                        totalRevenue,
                        completedCount,
                        failedCount
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment statistics");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
