using AutoMapper;
using Gymunity.Application.Contracts.ExternalServices;
using Gymunity.Application.Contracts.Services.Trainer;
using Gymunity.Application.DTOs.Trainers;
using Gymunity.Application.Specifications.Admin;
using Gymunity.Application.Specifications.Client;
using Gymunity.Application.Specifications.Trainer;
using Gymunity.Domain;
using Gymunity.Domain.Entities;
using Gymunity.Domain.Entities.Identity;
using Gymunity.Domain.Entities.Trainer;
using Gymunity.Domain.Enums;
using Gymunity.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Gymunity.Application.Services.Trainer
{
    public class TrainerProfileService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IFileUploadService fileUploadService,
        UserManager<AppUser> userManager) : ITrainerProfileService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly IFileUploadService _fileUploadService = fileUploadService;
        private readonly UserManager<AppUser> _userManager = userManager;

        //public async Task<IEnumerable<TrainerProfileListResponse>> GetAllProfiles()
        //{
        //    var profileSpecs = new TrainerWithUsersAndProgramsSpecs();

        //    var trainerProfiles = (await _unitOfWork.Repository<TrainerProfile, ITrainerProfileRepository>()
        //                                           .GetAllWithSpecsAsync(profileSpecs))
        //                                           .Where(tp => !tp.IsDeleted)
        //                                           .Select(tp => _mapper.Map<TrainerProfileListResponse>(tp));

        //    return trainerProfiles;
        //}

        public async Task<IEnumerable<TrainerReviewResponse>?> GetAllReviews(int trainerProfileId)
        {
            var reviewsSpecs = new TrainerProfileByIdSpecs(trainerProfileId);
            var trainerReviews = (await _unitOfWork.Repository<TrainerProfile>()
                                                   .GetWithSpecsAsync(reviewsSpecs))?
                                                   .TrainerReviews.Select(tr => _mapper.Map<TrainerReviewResponse>(tr));
            return trainerReviews;
        }

        public async Task<TrainerFullProfileResponse?> GetFullProfileByUserIdAsync(string userId)
        {
            var profileSpecs = new TrainerWithUsersAndProgramsSpecs(tp => tp.UserId == userId && !tp.IsDeleted);

            var profile = await _unitOfWork.Repository<TrainerProfile, ITrainerProfileRepository>()
                                          .GetWithSpecsAsync(profileSpecs);

            if (profile == null) return null;

            var user = profile.User;

            return new TrainerFullProfileResponse
            {
                UserId = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName ?? string.Empty,
                ProfilePhotoUrl = user.ProfilePhotoUrl,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                IsVerified = user.IsVerified,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,

                TrainerProfileId = profile.Id,
                Handle = profile.Handle,
                Bio = profile.Bio,
                CoverImageUrl = profile.CoverImageUrl,
                VideoIntroUrl = profile.VideoIntroUrl,
                BrandingColors = profile.BrandingColors,
                IsProfileVerified = profile.IsVerified,
                VerifiedAt = profile.VerifiedAt,
                IsSuspended = profile.IsSuspended,
                SuspendedAt = profile.SuspendedAt,
                RatingAverage = profile.RatingAverage,
                TotalClients = profile.TotalClients,
                YearsExperience = profile.YearsExperience,
                StatusImageUrl = profile.StatusImageUrl,
                StatusDescription = profile.StatusDescription,
                ProfileCreatedAt = profile.CreatedAt,
                ProfileUpdatedAt = profile.UpdatedAt
            };
        }

        public async Task<TrainerProfileDetailResponse?> GetProfileByUserId(string userId)
        {
            var profileSpecs = new TrainerWithUsersAndProgramsSpecs(tp => tp.UserId == userId && !tp.IsDeleted);

            var profile = await _unitOfWork.Repository<TrainerProfile, ITrainerProfileRepository>()
                                          .GetWithSpecsAsync(profileSpecs);

            if (profile == null) return null;

            var dto = _mapper.Map<TrainerProfileDetailResponse>(profile);

            // compute reviews list
            var reviews = profile.TrainerReviews?.Where(r => !r.IsDeleted).ToList() ?? new List<TrainerReview>();
            dto.TotalReviewsCount = reviews.Count;
            dto.Reviews = reviews.Select(r => _mapper.Map<TrainerReviewResponse>(r)).ToList();

            // compute rating sum and average
            var ratingSum = reviews.Sum(r => r.Rating);
            dto.RatingSum = ratingSum;
            if (reviews.Count >0)
            {
                dto.RatingAverage = Math.Round((decimal)ratingSum / reviews.Count,2);
            }
            else
            {
                dto.RatingAverage =0m;
            }

            // compute available balance from completed payments for trainer's packages
            var packageIds = profile.Packages?.Select(p => p.Id).ToList() ?? [];
            decimal availableBalance =0m;
            if (packageIds.Count >0)
            {
                var subsWithPayments = await _unitOfWork.Repository<Subscription>().GetAllWithSpecsAsync(new ActiveSubscriptionsWithPaymentsSpecs());
                var filtered = subsWithPayments.Where(s => packageIds.Contains(s.PackageId)).ToList();

                var completedPayments = filtered.SelectMany(s => s.Payments ?? new List<Payment>())
                .Where(p => p.Status == PaymentStatus.Completed && !p.IsDeleted)
                .ToList();

                availableBalance = completedPayments.Sum(p => p.TrainerPayout);
            }
            dto.AvailableBalance = availableBalance;

            return dto;
        }

        public async Task<TrainerProfileDetailResponse?> GetProfileById(int id)
        {
            var profileSpecs = new TrainerWithUsersAndProgramsSpecs(tp => tp.Id == id && !tp.IsDeleted);

            var profile = await _unitOfWork.Repository<TrainerProfile, ITrainerProfileRepository>()
                                          .GetWithSpecsAsync(profileSpecs);

            return profile != null ? _mapper.Map<TrainerProfileDetailResponse>(profile) : null;
        }

        public async Task<TrainerProfileDetailResponse> CreateProfile(CreateTrainerProfileRequest request)
        {
            // Get repository
            var repository = _unitOfWork.Repository<TrainerProfile, ITrainerProfileRepository>();

            // Validate user exists to avoid FK conflict
            if (string.IsNullOrEmpty(request.UserId))
            {
                throw new InvalidOperationException("UserId is required.");
            }

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                throw new InvalidOperationException($"User '{request.UserId}' not found.");
            }

            // Check if user already has a trainer profile
            var existingProfile = await repository.GetWithSpecsAsync(
                new TrainerWithUsersAndProgramsSpecs(tp => tp.UserId == request.UserId && !tp.IsDeleted));

            if (existingProfile != null)
            {
                throw new InvalidOperationException($"User '{request.UserId}' already has a trainer profile.");
            }

            // Check if handle already exists
            if (await repository.HandleExistsAsync(request.Handle))
            {
                throw new InvalidOperationException($"Handle '{request.Handle}' already exists.");
            }

            // Map request to entity
            var profile = _mapper.Map<TrainerProfile>(request);

            // Ensure FK and navigation are set to the existing user
            profile.UserId = user.Id;
            profile.User = user;

            // Handle cover image upload
            if (request.CoverImage != null)
            {
                profile.CoverImageUrl = await _fileUploadService.UploadImageAsync(
                    request.CoverImage,
                    "trainers/covers");
            }

            // Add profile
            repository.Add(profile);
            await _unitOfWork.CompleteAsync();

            // Return created profile
            return await GetProfileById(profile.Id)
                   ?? throw new InvalidOperationException("Failed to retrieve created profile.");
        }

        public async Task<TrainerProfileDetailResponse> UpdateProfile(int profileId, UpdateTrainerProfileRequest request)
        {
            var profileSpecs = new TrainerWithUsersAndProgramsSpecs(tp => tp.Id == profileId && !tp.IsDeleted);
            var repository = _unitOfWork.Repository<TrainerProfile, ITrainerProfileRepository>();

            var profile = await repository.GetWithSpecsAsync(profileSpecs);

            if (profile == null)
            {
                throw new InvalidOperationException("Trainer profile not found.");
            }

            // Check if new handle already exists (if handle is being changed)
            if (!string.IsNullOrEmpty(request.Handle) && request.Handle != profile.Handle)
            {
                if (await repository.HandleExistsAsync(request.Handle))
                {
                    throw new InvalidOperationException($"Handle '{request.Handle}' already exists.");
                }
            }

            // Handle cover image upload
            if (request.CoverImage != null)
            {
                // Delete old image if exists
                if (!string.IsNullOrEmpty(profile.CoverImageUrl))
                {
                    _fileUploadService.DeleteImage(profile.CoverImageUrl);
                }

                profile.CoverImageUrl = await _fileUploadService.UploadImageAsync(
                    request.CoverImage,
                    "trainers/covers");
            }

            // Map updated values
            _mapper.Map(request, profile);
            profile.UpdatedAt = DateTimeOffset.UtcNow;

            repository.Update(profile);
            await _unitOfWork.CompleteAsync();

            return await GetProfileById(profile.Id)
                   ?? throw new InvalidOperationException("Failed to retrieve updated profile.");
        }

        public async Task<bool> DeleteProfile(int profileId)
        {
            var repository = _unitOfWork.Repository<TrainerProfile, ITrainerProfileRepository>();
            var profile = await repository.GetByIdAsync(profileId);

            if (profile == null || profile.IsDeleted)
            {
                return false;
            }

            // Soft delete
            profile.IsDeleted = true;
            profile.UpdatedAt = DateTimeOffset.UtcNow;

            repository.Update(profile);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        // Get subscribers (clients with active subscriptions) for a trainer
        public async Task<IReadOnlyList<SubscriberResponse>> GetSubscribersByTrainerIdAsync(string trainerId)
        {
            var profileId = (await GetProfileByUserId(trainerId))?.Id;
            if (profileId is null)
                return new List<SubscriberResponse>();
            // 1. get packages for trainer (use package repository helper)
            var pkgRepo = _unitOfWork.Repository<Package, IPackageRepository>();
            var packages = (await pkgRepo.GetByTrainerIdAsync(profileId.Value)).ToList();

            if (packages.Count == 0)
                return new List<SubscriberResponse>();

            var packageIds = packages.Select(p => p.Id).ToList();

            // 2. get active subscriptions for these packages
            var spec = new  SubscriptionsWithClientAndProgramSpecs(s => packageIds.Contains(s.PackageId) && s.Status == SubscriptionStatus.Active && s.CurrentPeriodEnd > DateTime.UtcNow);

            var subscriptions = await _unitOfWork.Repository<Subscription>().GetAllWithSpecsAsync(spec);

            // 3. project to response
            var result = subscriptions
                .Select(s => new SubscriberResponse
                {
                    ClientId = s.ClientId,
                    ClientName = s.Client?.FullName ?? string.Empty,
                    ClientEmail = s.Client?.Email ?? string.Empty,
                    PackageName = s.Package?.Name ?? string.Empty,
                    SubscriptionStartDate = s.StartDate,
                    SubscriptionEndDate = s.CurrentPeriodEnd,
                    Status = s.Status
                })
                .ToList();

            return result;
        }

        public async Task<TrainerProfileDetailResponse> UpdateStatus(int profileId, UpdateStatusRequest request)
        {
            var profileSpecs = new TrainerWithUsersAndProgramsSpecs(tp => tp.Id == profileId && !tp.IsDeleted);
            var repository = _unitOfWork.Repository<TrainerProfile, ITrainerProfileRepository>();

            var profile = await repository.GetWithSpecsAsync(profileSpecs);

            if (profile == null)
            {
                throw new InvalidOperationException("Trainer profile not found.");
            }

            // Handle status image upload
            if (request.StatusImage != null)
            {
                // Delete old status image if exists
                if (!string.IsNullOrEmpty(profile.StatusImageUrl))
                {
                    _fileUploadService.DeleteImage(profile.StatusImageUrl);
                }

                profile.StatusImageUrl = await _fileUploadService.UploadImageAsync(
                    request.StatusImage,
                    "trainers/status");
            }

            // Update status description
            if (request.StatusDescription != null)
            {
                profile.StatusDescription = request.StatusDescription;
            }

            profile.UpdatedAt = DateTimeOffset.UtcNow;

            repository.Update(profile);
            await _unitOfWork.CompleteAsync();

            return await GetProfileById(profile.Id)
                   ?? throw new InvalidOperationException("Failed to retrieve updated profile.");
        }

        public async Task<bool> DeleteStatus(int profileId)
        {
            var repository = _unitOfWork.Repository<TrainerProfile, ITrainerProfileRepository>();
            var profile = await repository.GetByIdAsync(profileId);

            if (profile == null || profile.IsDeleted)
            {
                return false;
            }

            // Delete status image if exists
            if (!string.IsNullOrEmpty(profile.StatusImageUrl))
            {
                _fileUploadService.DeleteImage(profile.StatusImageUrl);
            }

            // Clear status fields
            profile.StatusImageUrl = null;
            profile.StatusDescription = null;
            profile.UpdatedAt = DateTimeOffset.UtcNow;

            repository.Update(profile);
            await _unitOfWork.CompleteAsync();

            return true;
        }
    }
}
