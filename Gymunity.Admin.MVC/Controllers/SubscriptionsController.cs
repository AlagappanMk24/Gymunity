using Gymunity.Admin.MVC.ViewModels.Subscriptions;
using Gymunity.Application.Contracts.Services.Admin;
using Gymunity.Application.Specifications.Admin;
using Gymunity.Domain.Enums;
using ITI.Gymunity.FP.Application.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gymunity.Admin.MVC.Controllers
{
    /// <summary>
    /// Admin Subscriptions Controller
    /// Manages subscription lifecycle and monitoring
    /// </summary>
    [Authorize(Roles = "Admin")]
    [Route("admin")]
    public class SubscriptionsController(
        ILogger<SubscriptionsController> logger,
        ISubscriptionAdminService subscriptionService) : BaseAdminController
    {
        private readonly ILogger<SubscriptionsController> _logger = logger;
        private readonly ISubscriptionAdminService _subscriptionService = subscriptionService;

        /// <summary>
        /// Displays list of all subscriptions with filtering and pagination
        /// </summary>
        [HttpGet("subscriptions")]
        public async Task<IActionResult> Index(
            int pageNumber = 1,
            int pageSize = 10,
            SubscriptionStatus? status = null)
        {
            try
            {
                SetPageTitle("Manage Subscriptions");
                SetBreadcrumbs("Subscriptions");

                var specs = new SubscriptionFilterSpecs(
                    status: status,
                    pageNumber: pageNumber,
                    pageSize: pageSize);

                var subscriptions = await _subscriptionService.GetAllSubscriptionsAsync(specs);
                var totalCount = await _subscriptionService.GetSubscriptionCountAsync(specs);

                var model = new SubscriptionsListViewModel
                {
                    Subscriptions = subscriptions.ToList(),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    StatusFilter = status
                };

                _logger.LogInformation("Subscriptions list accessed by user: {User}", User.Identity?.Name);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading subscriptions list");
                ShowErrorMessage("An error occurred while loading subscriptions");
                return RedirectToDashboard();
            }
        }

        /// <summary>
        /// Displays detailed view of a specific subscription
        /// </summary>
        [HttpGet("subscriptions/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                SetPageTitle("Subscription Details");
                SetBreadcrumbs("Subscriptions", "Details");

                // Use the method that loads trainer information with the specification
                var subscription = await _subscriptionService.GetSubscriptionDetailsWithTrainerAsync(id);
                if (subscription == null)
                {
                    ShowErrorMessage("Subscription not found");
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogInformation("Subscription {SubscriptionId} details viewed by {User}", id, User.Identity?.Name);
                return View(subscription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading subscription details for ID {SubscriptionId}", id);
                ShowErrorMessage("An error occurred while loading subscription details");
                return RedirectToDashboard();
            }
        }

        /// <summary>
        /// Displays active subscriptions
        /// </summary>
        [HttpGet("subscriptions/active")]
        public async Task<IActionResult> Active(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                SetPageTitle("Active Subscriptions");
                SetBreadcrumbs("Subscriptions", "Active");

                var subscriptions = await _subscriptionService.GetActiveSubscriptionsAsync(pageNumber, pageSize);
                var totalCount = await _subscriptionService.GetActiveSubscriptionCountAsync();

                var model = new SubscriptionsListViewModel
                {
                    Subscriptions = subscriptions.ToList(),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    StatusFilter = SubscriptionStatus.Active
                };

                _logger.LogInformation("Active subscriptions list accessed by {User}", User.Identity?.Name);
                return View("Index", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading active subscriptions");
                ShowErrorMessage("An error occurred while loading active subscriptions");
                return RedirectToDashboard();
            }
        }

        /// <summary>
        /// Displays inactive/canceled subscriptions
        /// </summary>
        [HttpGet("subscriptions/inactive")]
        public async Task<IActionResult> Inactive(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                SetPageTitle("Inactive Subscriptions");
                SetBreadcrumbs("Subscriptions", "Inactive");

                var subscriptions = await _subscriptionService.GetInactiveSubscriptionsAsync(pageNumber, pageSize);
                var totalCount = await _subscriptionService.GetSubscriptionCountAsync(
                    new SubscriptionFilterSpecs(status: SubscriptionStatus.Canceled));

                var model = new SubscriptionsListViewModel
                {
                    Subscriptions = subscriptions.ToList(),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    StatusFilter = SubscriptionStatus.Canceled
                };

                _logger.LogInformation("Inactive subscriptions list accessed by {User}", User.Identity?.Name);
                return View("Index", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading inactive subscriptions");
                ShowErrorMessage("An error occurred while loading inactive subscriptions");
                return RedirectToDashboard();
            }
        }

        /// <summary>
        /// Displays unpaid subscriptions
        /// </summary>
        [HttpGet("subscriptions/unpaid")]
        public async Task<IActionResult> Unpaid(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                SetPageTitle("Unpaid Subscriptions");
                SetBreadcrumbs("Subscriptions", "Unpaid");

                var subscriptions = await _subscriptionService.GetUnpaidSubscriptionsAsync(pageNumber, pageSize);
                var totalCount = await _subscriptionService.GetSubscriptionCountAsync(
                    new SubscriptionFilterSpecs(status: SubscriptionStatus.Unpaid));

                var model = new SubscriptionsListViewModel
                {
                    Subscriptions = subscriptions.ToList(),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    StatusFilter = SubscriptionStatus.Unpaid
                };

                _logger.LogInformation("Unpaid subscriptions list accessed by {User}", User.Identity?.Name);
                return View("Index", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading unpaid subscriptions");
                ShowErrorMessage("An error occurred while loading unpaid subscriptions");
                return RedirectToDashboard();
            }
        }

        /// <summary>
        /// Cancels a subscription (admin action)
        /// </summary>
        [HttpPost("subscriptions/{id}/cancel")]
        public async Task<IActionResult> Cancel(int id, [FromBody] SubscriptionCancelRequest request)
        {
            try
            {
                var result = await _subscriptionService.CancelSubscriptionAsync(id, request?.Reason ?? "");
                if (!result)
                    return BadRequest(new { success = false, message = "Failed to cancel subscription" });

                ShowSuccessMessage("Subscription canceled successfully");
                _logger.LogInformation("Subscription {SubscriptionId} canceled by {User}", id, User.Identity?.Name);
                return Ok(new { success = true, message = "Subscription canceled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling subscription {SubscriptionId}", id);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Displays subscriptions expiring soon
        /// </summary>
        [HttpGet("subscriptions/expiring-soon")]
        public async Task<IActionResult> ExpiringSoon()
        {
            try
            {
                SetPageTitle("Subscriptions Expiring Soon");
                SetBreadcrumbs("Subscriptions", "Expiring Soon");

                var subscriptions = await _subscriptionService.GetExpiringSoonSubscriptionsAsync();

                var model = new SubscriptionsListViewModel
                {
                    Subscriptions = subscriptions.ToList(),
                    PageNumber = 1,
                    PageSize = subscriptions.Count(),
                    TotalCount = subscriptions.Count()
                };

                _logger.LogInformation("Expiring soon subscriptions list accessed by {User}", User.Identity?.Name);
                return View("Index", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading expiring subscriptions");
                ShowErrorMessage("An error occurred while loading expiring subscriptions");
                return RedirectToDashboard();
            }
        }

        /// <summary>
        /// AJAX endpoint for DataTable - Returns subscriptions as JSON
        /// </summary>
        [HttpGet("subscriptions/list-json")]
        public async Task<IActionResult> GetSubscriptionsJson(
            int draw = 1,
            int start = 0,
            int length = 10,
            string? status = null)
        {
            try
            {
                var pageNumber = start / length + 1;
                SubscriptionStatus? subscriptionStatus = null;

                if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<SubscriptionStatus>(status, out var parsedStatus))
                {
                    subscriptionStatus = parsedStatus;
                }

                var specs = new SubscriptionFilterSpecs(
                    status: subscriptionStatus,
                    pageNumber: pageNumber,
                    pageSize: length);

                var subscriptions = await _subscriptionService.GetAllSubscriptionsAsync(specs);
                var totalCount = await _subscriptionService.GetSubscriptionCountAsync(new SubscriptionFilterSpecs());

                return Json(new
                {
                    draw,
                    recordsTotal = totalCount,
                    recordsFiltered = totalCount,
                    data = subscriptions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscriptions JSON");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// AJAX endpoint for advanced subscription filtering with multiple criteria
        /// Supports status, trainer, search term, and date range filters
        /// </summary>
        [HttpPost("subscriptions/filter")]
        public async Task<IActionResult> FilterSubscriptions([FromBody] SubscriptionFilterRequest filterRequest)
        {
            try
            {
                if (filterRequest == null)
                {
                    return BadRequest(new { success = false, message = "Invalid filter request" });
                }

                // Get filtered subscriptions using specification
                var subscriptions = await _subscriptionService.GetSubscriptionsWithAdvancedFilterAsync(
                    status: filterRequest.Status,
                    trainerId: filterRequest.TrainerId,
                    searchTerm: filterRequest.SearchTerm,
                    startDate: filterRequest.StartDate,
                    endDate: filterRequest.EndDate,
                    pageNumber: filterRequest.PageNumber,
                    pageSize: filterRequest.PageSize);

                // Get total count for pagination
                var totalCount = await _subscriptionService.GetSubscriptionCountWithAdvancedFilterAsync(
                    status: filterRequest.Status,
                    trainerId: filterRequest.TrainerId,
                    searchTerm: filterRequest.SearchTerm,
                    startDate: filterRequest.StartDate,
                    endDate: filterRequest.EndDate);

                int totalPages = (int)Math.Ceiling((decimal)totalCount / filterRequest.PageSize);

                var filterResponse = new SubscriptionFilterResponse
                {
                    Subscriptions = subscriptions.Select(s => new SubscriptionFilterItem
                    {
                        Id = s.Id,
                        ClientId = 0,
                        ClientName = s.TrainerName ?? "Unknown",
                        ClientEmail = "",
                        PackageId = s.PackageId,
                        PackageName = s.PackageName ?? "Unknown",
                        TrainerId = 0,
                        TrainerName = s.TrainerName ?? "Unknown",
                        Amount = s.AmountPaid,
                        Status = s.Status,
                        StartDate = s.StartDate,
                        CurrentPeriodEnd = s.CurrentPeriodEnd,
                        CreatedAt = s.StartDate
                    }),
                    TotalCount = totalCount,
                    PageNumber = filterRequest.PageNumber,
                    PageSize = filterRequest.PageSize,
                    HasNextPage = filterRequest.PageNumber < totalPages,
                    HasPreviousPage = filterRequest.PageNumber > 1,
                    TotalPages = totalPages
                };

                _logger.LogDebug("Applied subscription filters: Status={Status}, Trainer={Trainer}, Search={Search}",
                    filterRequest.Status, filterRequest.TrainerId, filterRequest.SearchTerm);

                return Json(new
                {
                    success = true,
                    data = filterResponse
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering subscriptions");
                return BadRequest(new
                {
                    success = false,
                    message = "Error applying filters",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// AJAX endpoint for getting subscription details with trainer information
        /// </summary>
        [HttpGet("subscriptions/{id}/details-ajax")]
        public async Task<IActionResult> GetSubscriptionDetailsAjax(int id)
        {
            try
            {
                var subscription = await _subscriptionService.GetSubscriptionDetailsWithTrainerAsync(id);
                if (subscription == null)
                {
                    return NotFound(new { success = false, message = "Subscription not found" });
                }

                return Json(new
                {
                    success = true,
                    data = subscription
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscription details for ID {SubscriptionId}", id);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// AJAX endpoint for getting subscription statistics
        /// </summary>
        [HttpGet("subscriptions/stats")]
        public async Task<IActionResult> GetSubscriptionStats()
        {
            try
            {
                var (activeCount, unpaidCount, canceledCount, totalRevenue) = 
                    await _subscriptionService.GetSubscriptionStatsAsync();

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        activeCount,
                        unpaidCount,
                        canceledCount,
                        totalRevenue
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscription statistics");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
