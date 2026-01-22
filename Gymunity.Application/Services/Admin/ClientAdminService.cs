using Gymunity.Application.Contracts.Services.Admin;
using Gymunity.Application.DTOs.ClientDto;
using Gymunity.Application.Specifications.Admin;
using Gymunity.Domain;
using Gymunity.Domain.Entities;
using Gymunity.Domain.Entities.Client;
using Gymunity.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Gymunity.Application.Services.Admin
{
    /// <summary>
    /// Admin service for managing client profiles and accounts
    /// </summary>
    public class ClientAdminService(IUnitOfWork unitOfWork, ILogger<ClientAdminService> logger) : IClientAdminService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<ClientAdminService> _logger = logger;

        /// <summary>
        /// Get all clients with filtering and pagination
        /// </summary>
        public async Task<IEnumerable<ClientListItemDto>> GetAllClientsAsync(ClientFilterSpecs specs)
        {
            try
            {
                var clients = await _unitOfWork
                    .Repository<ClientProfile>()
                    .GetAllWithSpecsAsync(specs);

                return [.. clients.Select(c => new ClientListItemDto
                {
                    UserId = c.UserId,
                    FullName = c.User?.FullName ?? "Unknown",
                    UserName = c.User?.UserName,
                    Email = c.User?.Email,
                    ProfilePhotoUrl = c.User?.ProfilePhotoUrl,
                    CreatedAt = c.User?.CreatedAt ?? DateTime.UtcNow
                })];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all clients");
                throw;
            }
        }

        /// <summary>
        /// Get client by ID with full details including subscriptions and payments
        /// Uses specifications for efficient data loading with eager loading
        /// </summary>
        public async Task<ClientDetailedResponse?> GetClientByIdAsync(string clientId)
        {
            try
            {
                // Get client profile with related user data using specification
                var clientSpec = new ClientDetailSpecs(clientId);
                var clientProfile = await _unitOfWork
                    .Repository<ClientProfile>()
                    .GetWithSpecsAsync(clientSpec);

                if (clientProfile == null)
                {
                    _logger.LogWarning("Client with ID {ClientId} not found", clientId);
                    return null;
                }

                // Get Subscriptions with all related data using specification
                var subscriptionSpec = new ClientSubscriptionsSpecs(clientId);
                var clientSubs = await _unitOfWork
                    .Repository<Subscription>()
                    .GetAllWithSpecsAsync(subscriptionSpec);

                // Get Payments with all related data using specification
                var paymentSpec = new ClientPaymentsSpecs(clientId);
                var clientPayments = await _unitOfWork
                    .Repository<Payment>()
                    .GetAllWithSpecsAsync(paymentSpec);

                // Build response
                var response = new ClientDetailedResponse
                {
                    Id = clientProfile.Id,
                    UserId = clientProfile.UserId,
                    FullName = clientProfile.User?.FullName ?? "Unknown",
                    Email = clientProfile.User?.Email,
                    UserName = clientProfile.User?.UserName,
                    ProfilePhotoUrl = clientProfile.User?.ProfilePhotoUrl,
                    CreatedAt = clientProfile.User?.CreatedAt ?? DateTime.UtcNow,
                    LastLoginAt = clientProfile.User?.LastLoginAt,
                    IsVerified = clientProfile.User?.IsVerified ?? false,
                    
                    // Client Profile Data
                    HeightCm = clientProfile.HeightCm,
                    StartingWeightKg = clientProfile.StartingWeightKg,
                    Gender = clientProfile.Gender,
                    Goal = clientProfile.Goal,
                    ExperienceLevel = clientProfile.ExperienceLevel,
                    
                    // Subscription Data
                    TotalSubscriptions = clientSubs.Count(),
                    ActiveSubscriptions = clientSubs.Count(s => s.Status == SubscriptionStatus.Active),
                    
                    // Payment Data
                    TotalPaymentsCount = clientPayments.Count(),
                    TotalAmountPaid = clientPayments.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.Amount),
                    
                    // Collections - map from eager-loaded data
                    Subscriptions = clientSubs.Select(s => new ClientSubscriptionDto
                    {
                        Id = s.Id,
                        PackageName = s.Package?.Name ?? "Unknown Package",
                        TrainerHandle = s.Package?.Trainer?.Handle ?? "Unknown",
                        Status = s.Status.ToString(),
                        AmountPaid = s.Package?.PriceMonthly ?? 0,
                        Currency = s.Package?.Currency ?? "EGP",
                        IsAnnual = s.IsAnnual,
                        StartDate = s.StartDate,
                        CurrentPeriodEnd = s.CurrentPeriodEnd,
                        CanceledAt = s.CanceledAt
                    }).ToList(),
                    
                    Payments = clientPayments.Select(p => new ClientPaymentDto
                    {
                        Id = p.Id,
                        PackageName = p.Subscription?.Package?.Name ?? "Unknown Package",
                        TrainerName = p.Subscription?.Package?.Trainer?.User?.FullName ?? "Unknown",
                        Amount = p.Amount,
                        Currency = p.Currency,
                        Status = p.Status.ToString(),
                        Method = p.Method.ToString(),
                        CreatedAt = p.CreatedAt,
                        PaidAt = p.PaidAt,
                        FailureReason = p.FailureReason
                    }).ToList()
                };

                _logger.LogInformation("Retrieved client details for {ClientId}", clientId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving client details for ID {ClientId}", clientId);
                throw;
            }
        }

        /// <summary>
        /// Get total count of clients matching specifications
        /// </summary>
        public async Task<int> GetClientCountAsync(ClientFilterSpecs specs)
        {
            try
            {
                return await _unitOfWork
                    .Repository<ClientProfile>()
                    .GetCountWithspecsAsync(specs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting client count");
                throw;
            }
        }

        /// <summary>
        /// Get total count of all clients
        /// </summary>
        public async Task<int> GetTotalClientCountAsync()
        {
            try
            {
                var specs = new ClientFilterSpecs();
                return await _unitOfWork
                    .Repository<ClientProfile>()
                    .GetCountWithspecsAsync(specs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total client count");
                throw;
            }
        }

        /// <summary>
        /// Get active clients
        /// </summary>
        public async Task<IEnumerable<ClientListItemDto>> GetActiveClientsAsync(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var specs = new ClientFilterSpecs(pageNumber: pageNumber, pageSize: pageSize);
                var clients = await _unitOfWork
                    .Repository<ClientProfile>()
                    .GetAllWithSpecsAsync(specs);

                return clients.Select(c => new ClientListItemDto
                {
                    UserId = c.UserId,
                    FullName = c.User?.FullName ?? "Unknown",
                    UserName = c.User?.UserName,
                    Email = c.User?.Email,
                    ProfilePhotoUrl = c.User?.ProfilePhotoUrl,
                    CreatedAt = c.User?.CreatedAt ?? DateTime.UtcNow
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active clients");
                throw;
            }
        }

        /// <summary>
        /// Get inactive/suspended clients
        /// </summary>
        public async Task<IEnumerable<ClientListItemDto>> GetInactiveClientsAsync(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var specs = new ClientFilterSpecs(pageNumber: pageNumber, pageSize: pageSize);
                var clients = await _unitOfWork
                    .Repository<ClientProfile>()
                    .GetAllWithSpecsAsync(specs);

                return clients.Select(c => new ClientListItemDto
                {
                    UserId = c.UserId,
                    FullName = c.User?.FullName ?? "Unknown",
                    UserName = c.User?.UserName,
                    Email = c.User?.Email,
                    ProfilePhotoUrl = c.User?.ProfilePhotoUrl,
                    CreatedAt = c.User?.CreatedAt ?? DateTime.UtcNow
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving inactive clients");
                throw;
            }
        }

        /// <summary>
        /// Search clients by name, email, or username
        /// </summary>
        public async Task<IEnumerable<ClientListItemDto>> SearchClientsAsync(
            string searchTerm,
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                var specs = new ClientFilterSpecs(pageNumber: pageNumber, pageSize: pageSize, searchTerm: searchTerm);
                var clients = await _unitOfWork
                    .Repository<ClientProfile>()
                    .GetAllWithSpecsAsync(specs);

                return clients.Select(c => new ClientListItemDto
                {
                    UserId = c.UserId,
                    FullName = c.User?.FullName ?? "Unknown",
                    UserName = c.User?.UserName,
                    Email = c.User?.Email,
                    ProfilePhotoUrl = c.User?.ProfilePhotoUrl,
                    CreatedAt = c.User?.CreatedAt ?? DateTime.UtcNow
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching clients with term {SearchTerm}", searchTerm);
                throw;
            }
        }

        /// <summary>
        /// Suspend a client account
        /// </summary>
        public async Task<bool> SuspendClientAsync(int clientId)
        {
            try
            {
                var client = await _unitOfWork
                    .Repository<ClientProfile>()
                    .GetByIdAsync(clientId);

                if (client == null)
                {
                    _logger.LogWarning("Client with ID {ClientId} not found for suspension", clientId);
                    return false;
                }

                client.IsDeleted = true;
                _unitOfWork.Repository<ClientProfile>().Update(client);
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation("Client {ClientId} suspended", clientId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error suspending client {ClientId}", clientId);
                throw;
            }
        }

        /// <summary>
        /// Reactivate a suspended client account
        /// </summary>
        public async Task<bool> ReactivateClientAsync(int clientId)
        {
            try
            {
                var client = await _unitOfWork
                    .Repository<ClientProfile>()
                    .GetByIdAsync(clientId);

                if (client == null)
                {
                    _logger.LogWarning("Client with ID {ClientId} not found for reactivation", clientId);
                    return false;
                }

                client.IsDeleted = false;
                _unitOfWork.Repository<ClientProfile>().Update(client);
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation("Client {ClientId} reactivated", clientId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reactivating client {ClientId}", clientId);
                throw;
            }
        }
    }

    /// <summary>
    /// Detailed client response DTO with subscriptions and payments
    /// </summary>
    public class ClientDetailedResponse
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? ProfilePhotoUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsVerified { get; set; }

        // Client Profile Data
        public int? HeightCm { get; set; }
        public decimal? StartingWeightKg { get; set; }
        public string? Gender { get; set; }
        public string? Goal { get; set; }
        public string? ExperienceLevel { get; set; }

        // Statistics
        public int TotalSubscriptions { get; set; }
        public int ActiveSubscriptions { get; set; }
        public int TotalPaymentsCount { get; set; }
        public decimal TotalAmountPaid { get; set; }

        // Collections
        public List<ClientSubscriptionDto> Subscriptions { get; set; } = new();
        public List<ClientPaymentDto> Payments { get; set; } = new();
    }

    /// <summary>
    /// Client subscription DTO for detailed view
    /// </summary>
    public class ClientSubscriptionDto
    {
        public int Id { get; set; }
        public string PackageName { get; set; } = null!;
        public string TrainerHandle { get; set; } = null!;
        public string Status { get; set; } = null!;
        public decimal AmountPaid { get; set; }
        public string Currency { get; set; } = "EGP";
        public bool IsAnnual { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime CurrentPeriodEnd { get; set; }
        public DateTime? CanceledAt { get; set; }
    }

    /// <summary>
    /// Client payment DTO for detailed view
    /// </summary>
    public class ClientPaymentDto
    {
        public int Id { get; set; }
        public string PackageName { get; set; } = null!;
        public string TrainerName { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EGP";
        public string Status { get; set; } = null!;
        public string Method { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? FailureReason { get; set; }
    }
}
