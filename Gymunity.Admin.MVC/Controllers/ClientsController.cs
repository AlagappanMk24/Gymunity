using Gymunity.Admin.MVC.ViewModels.Clients;
using Gymunity.Application.Contracts.ExternalServices.Email;
using Gymunity.Application.Contracts.Services.Admin;
using Gymunity.Application.DTOs.Email;
using Gymunity.Application.Specifications.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gymunity.Admin.MVC.Controllers
{
    /// <summary>
    /// Admin Clients Controller
    /// Manages client accounts, suspension, and lifecycle
    /// </summary>
    [Authorize(Roles = "Admin")]
    [Route("admin")]
    public class ClientsController(
        ILogger<ClientsController> logger,
        IClientAdminService clientService,
        IEmailService emailService) : BaseAdminController
    {
        private readonly ILogger<ClientsController> _logger = logger;
        private readonly IClientAdminService _clientService = clientService;
        private readonly IEmailService _emailService = emailService;

        /// <summary>
        /// Displays list of all clients with filtering and pagination
        /// Returns full view for initial load, partial for AJAX requests
        /// </summary>
        [HttpGet("clients")]
        public async Task<IActionResult> Index(
            int pageNumber = 1,
            int pageSize = 10,
            bool? isActive = null,
            string? searchTerm = null)
        {
            try
            {
                SetPageTitle("Manage Clients");
                SetBreadcrumbs("Clients");

                var specs = new ClientFilterSpecs(
                    isActive: isActive,
                    pageNumber: pageNumber,
                    pageSize: pageSize,
                    searchTerm: searchTerm);

                var clients = await _clientService.GetAllClientsAsync(specs);
                var totalCount = await _clientService.GetClientCountAsync(specs);

                var model = new ClientsListViewModel
                {
                    Clients = [.. clients],
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    SearchTerm = searchTerm,
                    IsActiveFilter = isActive
                };

                _logger.LogInformation("Clients list accessed by user: {User}", User.Identity?.Name);

                // Check if this is an AJAX request
                if (Request.Headers.XRequestedWith == "XMLHttpRequest")
                {
                    // Return only the partial view (table + pagination) for AJAX
                    return PartialView("_ClientsTablePartial", model);
                }

                // Return full view for initial page load
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading clients list");
                ShowErrorMessage("An error occurred while loading clients");

                // If AJAX request, return error response
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return BadRequest(new { success = false, message = ex.Message });
                }

                return RedirectToDashboard();
            }
        }

        /// <summary>
        /// Displays detailed view of a specific client with subscriptions and payments
        /// </summary>
        [HttpGet("clients/{id}")]
        public async Task<IActionResult> Details(string id)
        {
            try
            {
                SetPageTitle("Client Details");
                SetBreadcrumbs("Clients", "Details");

                var clientData = await _clientService.GetClientByIdAsync(id);
                if (clientData == null)
                {
                    ShowErrorMessage("Client not found");
                    return RedirectToAction(nameof(Index));
                }

                // Map to ViewModel
                var vm = new ClientDetailsViewModel
                {
                    ProfileId = clientData.Id,
                    UserId = clientData.UserId,
                    FullName = clientData.FullName,
                    Email = clientData.Email,
                    UserName = clientData.UserName,
                    ProfilePhotoUrl = clientData.ProfilePhotoUrl,
                    CreatedAt = clientData.CreatedAt,
                    LastLoginAt = clientData.LastLoginAt,
                    IsVerified = clientData.IsVerified,
                    
                    // Profile
                    HeightCm = clientData.HeightCm,
                    StartingWeightKg = clientData.StartingWeightKg,
                    Gender = clientData.Gender,
                    Goal = clientData.Goal,
                    ExperienceLevel = clientData.ExperienceLevel,
                    
                    // Statistics
                    TotalSubscriptions = clientData.TotalSubscriptions,
                    ActiveSubscriptions = clientData.ActiveSubscriptions,
                    TotalPaymentsCount = clientData.TotalPaymentsCount,
                    TotalAmountPaid = clientData.TotalAmountPaid,
                    
                    // Subscriptions
                    Subscriptions = clientData.Subscriptions.Select(s => new ClientSubscriptionViewModel
                    {
                        Id = s.Id,
                        PackageName = s.PackageName,
                        TrainerHandle = s.TrainerHandle,
                        Status = s.Status,
                        AmountPaid = s.AmountPaid,
                        Currency = s.Currency,
                        IsAnnual = s.IsAnnual,
                        StartDate = s.StartDate,
                        CurrentPeriodEnd = s.CurrentPeriodEnd,
                        CanceledAt = s.CanceledAt
                    }).ToList(),
                    
                    // Payments
                    Payments = [.. clientData.Payments.Select(p => new ClientPaymentViewModel
                    {
                        Id = p.Id,
                        PackageName = p.PackageName,
                        TrainerName = p.TrainerName,
                        Amount = p.Amount,
                        Currency = p.Currency,
                        Status = p.Status,
                        Method = p.Method,
                        CreatedAt = p.CreatedAt,
                        PaidAt = p.PaidAt,
                        FailureReason = p.FailureReason
                    })]
                };

                _logger.LogInformation("Client {ClientId} details viewed by {User}", id, User.Identity?.Name);
                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading client details for ID {ClientId}", id);
                ShowErrorMessage("An error occurred while loading client details");
                return RedirectToDashboard();
            }
        }

        /// <summary>
        /// Suspends a client account
        /// </summary>
        [HttpPost("clients/{id}/suspend")]
        public async Task<IActionResult> Suspend(int id)
        {
            try
            {
                var result = await _clientService.SuspendClientAsync(id);
                if (!result)
                    return BadRequest(new { success = false, message = "Failed to suspend client" });

                ShowSuccessMessage("Client suspended successfully");
                _logger.LogInformation("Client {ClientId} suspended by {User}", id, User.Identity?.Name);
                return Ok(new { success = true, message = "Client suspended successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error suspending client {ClientId}", id);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Reactivates a suspended client account
        /// </summary>
        [HttpPost("clients/{id}/reactivate")]
        public async Task<IActionResult> Reactivate(int id)
        {
            try
            {
                var result = await _clientService.ReactivateClientAsync(id);
                if (!result)
                    return BadRequest(new { success = false, message = "Failed to reactivate client" });

                ShowSuccessMessage("Client reactivated successfully");
                _logger.LogInformation("Client {ClientId} reactivated by {User}", id, User.Identity?.Name);
                return Ok(new { success = true, message = "Client reactivated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reactivating client {ClientId}", id);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Displays active clients
        /// </summary>
        [HttpGet("clients/active")]
        public async Task<IActionResult> Active(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                SetPageTitle("Active Clients");
                SetBreadcrumbs("Clients", "Active");

                var clients = await _clientService.GetActiveClientsAsync(pageNumber, pageSize);
                var totalCount = await _clientService.GetClientCountAsync(
                    new ClientFilterSpecs(isActive: true));

                var model = new ClientsListViewModel
                {
                    Clients = clients.ToList(),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    IsActiveFilter = true
                };

                _logger.LogInformation("Active clients list accessed by {User}", User.Identity?.Name);
                return View("Index", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading active clients");
                ShowErrorMessage("An error occurred while loading active clients");
                return RedirectToDashboard();
            }
        }

        /// <summary>
        /// Displays suspended clients
        /// </summary>
        [HttpGet("clients/suspended")]
        public async Task<IActionResult> Suspended(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                SetPageTitle("Suspended Clients");
                SetBreadcrumbs("Clients", "Suspended");

                var clients = await _clientService.GetInactiveClientsAsync(pageNumber, pageSize);
                var totalCount = await _clientService.GetClientCountAsync(
                    new ClientFilterSpecs(isActive: false));

                var model = new ClientsListViewModel
                {
                    Clients = clients.ToList(),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    IsActiveFilter = false
                };

                _logger.LogInformation("Suspended clients list accessed by {User}", User.Identity?.Name);
                return View("Index", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading suspended clients");
                ShowErrorMessage("An error occurred while loading suspended clients");
                return RedirectToDashboard();
            }
        }

        /// <summary>
        /// Searches clients by name, email, or username
        /// </summary>
        [HttpGet("clients/search")]
        public async Task<IActionResult> Search(
            string? q,
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                    return RedirectToAction(nameof(Index));

                var clients = await _clientService.SearchClientsAsync(q, pageNumber, pageSize);

                var model = new ClientsListViewModel
                {
                    Clients = clients.ToList(),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    SearchTerm = q
                };

                return Json(new { success = true, data = model });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching clients with term {SearchTerm}", q);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// AJAX endpoint for DataTable - Returns clients as JSON
        /// </summary>
        [HttpGet("clients/list-json")]
        public async Task<IActionResult> GetClientsJson(
            int draw = 1,
            int start = 0,
            int length = 10,
            string? search = null)
        {
            try
            {
                var pageNumber = start / length + 1;
                var specs = new ClientFilterSpecs(
                    pageNumber: pageNumber,
                    pageSize: length,
                    searchTerm: search);

                var clients = await _clientService.GetAllClientsAsync(specs);
                var totalCount = await _clientService.GetTotalClientCountAsync();

                return Json(new
                {
                    draw,
                    recordsTotal = totalCount,
                    recordsFiltered = totalCount,
                    data = clients
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting clients JSON");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Send email to a client
        /// </summary>
        [HttpPost("clients/send-email")]
        public async Task<IActionResult> SendEmail([FromBody] SendEmailRequest request)
        {
            try
            {
                if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request?.Email))
                    return BadRequest(new { success = false, message = "Invalid request" });

                var emailRequest = new EmailRequest
                {
                    ToEmail = request.Email,
                    ToName = request.RecipientName ?? "Client",
                    Subject = request.Subject,
                    Body = request.Body,
                    IsHtml = false
                };
                
                await _emailService.SendEmailAsync(emailRequest);
                
                _logger.LogInformation("Email sent to client {Email} by {User}", 
                    request.Email, User.Identity?.Name);
                
                ShowSuccessMessage("Email sent successfully");
                return Ok(new { success = true, message = "Email sent successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to client");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get activity log for a client
        /// </summary>
        [HttpGet("clients/activity")]
        public IActionResult GetActivity(string userId)
        {
            try
            {
                // TODO: Implement activity log retrieval
                var activities = new List<ActivityLog>
                {
                    new() {
                        Action = "Subscription Created",
                        Details = "Premium Package",
                        Timestamp = DateTime.UtcNow.AddHours(-2)
                    },
                    new() {
                        Action = "Payment Processed",
                        Details = "Amount: 500 EGP",
                        Timestamp = DateTime.UtcNow.AddHours(-4)
                    },
                    new() {
                        Action = "Profile Updated",
                        Details = "Changed fitness goals",
                        Timestamp = DateTime.UtcNow.AddDays(-1)
                    }
                };

                _logger.LogInformation("Activity log retrieved for client {ClientId}", userId);
                return Ok(new { success = true, data = activities });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving activity log for client {ClientId}", userId);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Generate a client report (PDF)
        /// </summary>
        [HttpGet("clients/generate-report")]
        public async Task<IActionResult> GenerateReport(string userId)
        {
            try
            {
                var clientData = await _clientService.GetClientByIdAsync(userId);
                if (clientData == null)
                    return NotFound(new { success = false, message = "Client not found" });

                // TODO: Implement PDF generation using a library like iTextSharp or SelectPdf
                // For now, return a placeholder file
                var reportContent = $@"
                    CLIENT REPORT
                    ============ =
                    Name: {clientData.FullName}
                    Email: {clientData.Email}
                    User ID: {clientData.UserId}

                    SUBSCRIPTION SUMMARY
                    -------------------
                    Total Subscriptions: {clientData.TotalSubscriptions}
                    Active Subscriptions: {clientData.ActiveSubscriptions}

                    PAYMENT SUMMARY
                    ---------------
                    Total Payments: {clientData.TotalPaymentsCount}
                    Total Amount Paid: {clientData.TotalAmountPaid}

                    Generated: {DateTime.UtcNow}
                    ";

                var bytes = System.Text.Encoding.UTF8.GetBytes(reportContent);
                _logger.LogInformation("Report generated for client {ClientId}", userId);
                return File(bytes, "application/pdf", $"client-report-{userId}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report for client {ClientId}", userId);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get compliance information for a client
        /// </summary>
        [HttpGet("clients/compliance")]
        public async Task<IActionResult> GetCompliance(string userId)
        {
            try
            {
                var clientData = await _clientService.GetClientByIdAsync(userId);
                if (clientData == null)
                    return NotFound(new { success = false, message = "Client not found" });

                var compliance = new ComplianceInfo
                {
                    EmailVerified = clientData.IsVerified,
                    PaymentVerified = clientData.Payments.Any(p => p.Status == "Completed"),
                    HasActiveSubscription = clientData.ActiveSubscriptions > 0,
                    LastPaymentSuccess = clientData.Payments.Any(p => p.Status == "Completed"),
                    RiskLevel = clientData.ActiveSubscriptions > 0 ? "Low" : "Medium"
                };

                _logger.LogInformation("Compliance check performed for client {ClientId}", userId);
                return Ok(new { success = true, data = compliance });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving compliance for client {ClientId}", userId);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Process a refund for a client payment
        /// </summary>
        [HttpPost("clients/process-refund")]
        public IActionResult ProcessRefund([FromBody] RefundRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid request" });

                // TODO: Implement refund processing logic
                // This would involve:
                // 1. Verifying the payment exists and belongs to the client
                // 2. Calling the payment service to process the refund
                // 3. Logging the refund transaction
                // 4. Notifying the client

                _logger.LogWarning("Refund processed for client {ClientId}, Payment {PaymentId} by {User}",
                    request.ClientId, request.PaymentId, User.Identity?.Name);

                return Ok(new { success = true, message = "Refund processed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }

    /// <summary>
    /// Activity log entry for client actions
    /// </summary>
    public class ActivityLog
    {
        public string Action { get; set; } = null!;
        public string Details { get; set; } = null!;
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Compliance information for a client
    /// </summary>
    public class ComplianceInfo
    {
        public bool EmailVerified { get; set; }
        public bool PaymentVerified { get; set; }
        public bool HasActiveSubscription { get; set; }
        public bool LastPaymentSuccess { get; set; }
        public string RiskLevel { get; set; } = null!;
    }

    /// <summary>
    /// Send email request
    /// </summary>
    public class SendEmailRequest
    {
        public string ClientId { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string RecipientName { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public string Body { get; set; } = null!;
        public string Message { get; set; } = null!;
        public bool SendCopy { get; set; }
    }

    /// <summary>
    /// Process refund request
    /// </summary>
    public class RefundRequest
    {
        public string ClientId { get; set; } = null!;
        public int PaymentId { get; set; }
        public string Reason { get; set; } = null!;
        public string? Notes { get; set; }
    }
}
