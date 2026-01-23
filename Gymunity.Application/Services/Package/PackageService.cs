using AutoMapper;
using Gymunity.Application.Contracts.ExternalServices;
using Gymunity.Application.Contracts.Services.Packages;
using Gymunity.Application.DTOs.Packages;
using Gymunity.Application.DTOs.Program;
using Gymunity.Application.DTOs.Trainers;
using Gymunity.Application.Specifications.Admin;
using Gymunity.Domain;
using Gymunity.Domain.Entities;
using Gymunity.Domain.Entities.ProgramAggregate;
using Gymunity.Domain.Entities.Trainer;
using Gymunity.Domain.Enums;
using Gymunity.Domain.Interfaces;
using ITI.Gymunity.FP.Application.Specefications.ClientSpecification;
using Microsoft.EntityFrameworkCore;

namespace Gymunity.Application.Services.Packages
{
    public class PackageService(IUnitOfWork unitOfWork, IMapper mapper, IImageUrlResolver imageUrlResolver) : IPackageService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly IImageUrlResolver _imageResolver = imageUrlResolver;

        public async Task<IReadOnlyList<PackageResponse>> GetAllForTrainerAsync(int trainerId)
        {
            var repo = _unitOfWork.Repository<Package, IPackageRepository>();
            var list = (await repo.GetByTrainerIdAsync(trainerId)).ToList();
            var mapped = list.Select(p => _mapper.Map<PackageResponse>(p)).ToList();

            // Resolve thumbnail urls and include program details (PackagePrograms already included by repo)
            for (int i = 0; i < mapped.Count; i++)
            {
                var pkgDto = mapped[i];
                var pkgEntity = list[i];
                if (!string.IsNullOrWhiteSpace(pkgDto.ThumbnailUrl))
                    pkgDto.ThumbnailUrl = _imageResolver.ResolveImageUrl(pkgDto.ThumbnailUrl);

                var programIds = pkgEntity.PackagePrograms?.Where(pp => !pp.IsDeleted).Select(pp => pp.ProgramId).ToArray() ?? [];
                if (programIds.Length > 0)
                {
                    var programs = pkgEntity.PackagePrograms.Where(pp => !pp.IsDeleted).Select(pp => pp.Program).Where(pr => pr != null).ToArray();
                    pkgDto.Programs = [.. programs.Select(pr => _mapper.Map<ProgramGetAllResponse>(pr))];
                    pkgDto.ProgramIds = programs.Select(pr => pr.Id).ToArray();
                }
            }
            return mapped;
        }

        public async Task<PackageResponse?> GetByIdAsync(int id)
        {
            var repo = _unitOfWork.Repository<Package, IPackageRepository>();
            var p = await repo.GetByIdWithProgramsAsync(id);
            if (p == null) return null;
            var mapped = _mapper.Map<PackageResponse>(p);
            if (!string.IsNullOrWhiteSpace(mapped.ThumbnailUrl))
                mapped.ThumbnailUrl = _imageResolver.ResolveImageUrl(mapped.ThumbnailUrl);

            var programEntities = p.PackagePrograms?.Where(pp => !pp.IsDeleted).Select(pp => pp.Program).Where(pr => pr != null).ToArray() ?? new Program[0];
            if (programEntities.Length > 0)
            {
                mapped.Programs = programEntities.Select(pr => _mapper.Map<ProgramGetAllResponse>(pr)).ToArray();
                mapped.ProgramIds = [.. programEntities.Select(pr => pr.Id)];
            }

            return mapped;
        }

        public async Task<PackageResponse> CreateAsync(int trainerId, PackageCreateRequest request)
        {
            // fallback to base behavior for old DTO
            var v2 = new PackageCreateRequestV2
            {
                Name = request.Name,
                Description = request.Description,
                PriceMonthly = request.PriceMonthly,
                PriceYearly = request.PriceYearly,
                IsActive = request.IsActive,
                ThumbnailUrl = request.ThumbnailUrl,
                ProgramIds = request.ProgramIds ?? new int[0],
                ProgramNames = new string[0],
                IsAnnual = request.IsAnnual,
                PromoCode = request.PromoCode,
                TrainerProfileId = request.TrainerId
            };
            return await CreateAsyncV2(trainerId, v2);
        }

        public async Task<PackageResponse> CreateAsyncV2(int trainerId, PackageCreateRequestV2 request)
        {
            // validate trainer profile exists
            var profileRepo = _unitOfWork.Repository<TrainerProfile, ITrainerProfileRepository>();
            var profile = await profileRepo.GetByIdAsync(trainerId);
            if (profile == null)
            {
                throw new InvalidOperationException($"Trainer profile with id {trainerId} not found.");
            }

            // Check global duplicate package name (database has unique index on Name)
            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                var pkgRepoAll = _unitOfWork.Repository<Package>();
                var allPackages = await pkgRepoAll.GetAllAsync();
                var duplicate = allPackages.Any(p => string.Equals(p.Name?.Trim(), request.Name.Trim(), StringComparison.OrdinalIgnoreCase));
                if (duplicate)
                {
                    throw new InvalidOperationException($"A package with name '{request.Name}' already exists.");
                }
            }

            var entity = new Package
            {
                Name = request.Name,
                Description = request.Description,
                PriceMonthly = request.PriceMonthly,
                PriceYearly = request.PriceYearly,
                IsActive = request.IsActive,
                ThumbnailUrl = request.ThumbnailUrl,
                TrainerId = trainerId,
                //IsAnnual = request.IsAnnual,
                PromoCode = request.PromoCode ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            _unitOfWork.Repository<Package>().Add(entity);
            await _unitOfWork.CompleteAsync();

            // resolve programs from names and ids
            var programRepo = _unitOfWork.Repository<Program, IProgramRepository>();
            var resolvedProgramIds = new List<int>();
            if (request.ProgramIds != null && request.ProgramIds.Length > 0)
                resolvedProgramIds.AddRange(request.ProgramIds);

            if (request.ProgramNames != null && request.ProgramNames.Length > 0)
            {
                var allPrograms = await programRepo.GetAllAsync();
                foreach (var name in request.ProgramNames.Select(n => n.Trim()).Where(n => !string.IsNullOrEmpty(n)))
                {
                    // try exact match case-insensitive first
                    var found = allPrograms.FirstOrDefault(p => string.Equals(p.Title?.Trim(), name, StringComparison.OrdinalIgnoreCase));
                    if (found == null)
                    {
                        // fallback to contains
                        found = allPrograms.FirstOrDefault(p => p.Title != null && p.Title.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0);
                    }

                    if (found != null && !resolvedProgramIds.Contains(found.Id))
                        resolvedProgramIds.Add(found.Id);
                }
            }

            if (resolvedProgramIds.Any())
            {
                var packageProgramRepo = _unitOfWork.Repository<PackageProgram>();
                foreach (var pid in resolvedProgramIds)
                {
                    // avoid duplicates
                    var existing = (await packageProgramRepo.GetAllAsync()).FirstOrDefault(pp => pp.PackageId == entity.Id && pp.ProgramId == pid);
                    if (existing == null)
                    {
                        var pp = new PackageProgram { PackageId = entity.Id, ProgramId = pid };
                        packageProgramRepo.Add(pp);
                    }
                }

                await _unitOfWork.CompleteAsync();
            }

            // reload package with includes to ensure navigation populated
            var repoWithIncludes = _unitOfWork.Repository<Package, IPackageRepository>();
            var saved = await repoWithIncludes.GetByIdWithProgramsAsync(entity.Id) ?? entity;
            var mapped = _mapper.Map<PackageResponse>(saved);
            if (!string.IsNullOrWhiteSpace(mapped.ThumbnailUrl))
                mapped.ThumbnailUrl = _imageResolver.ResolveImageUrl(mapped.ThumbnailUrl);

            var programEntities = saved.PackagePrograms?.Where(pp => !pp.IsDeleted).Select(pp => pp.Program).Where(pr => pr != null).ToArray() ?? new Program[0];
            if (programEntities.Length > 0)
            {
                mapped.Programs = programEntities.Select(pr => _mapper.Map<ProgramGetAllResponse>(pr)).ToArray();
                mapped.ProgramIds = programEntities.Select(pr => pr.Id).ToArray();
            }

            return mapped;
        }

        public async Task<bool> UpdateAsync(int id, PackageCreateRequest request)
        {
            try
            {
                // load package with related PackagePrograms to sync correctly
                var packageRepo = _unitOfWork.Repository<Package, IPackageRepository>();
                var entity = await packageRepo.GetByIdWithProgramsAsync(id);
                if (entity == null) return false;

                entity.Name = request.Name;
                entity.Description = request.Description;
                entity.PriceMonthly = request.PriceMonthly;
                entity.PriceYearly = request.PriceYearly;
                entity.IsActive = request.IsActive;
                entity.ThumbnailUrl = request.ThumbnailUrl;
                //entity.IsAnnual = request.IsAnnual;
                entity.PromoCode = request.PromoCode ?? entity.PromoCode ?? string.Empty;

                // use loaded navigation collection to avoid duplicates
                var existingProgramLinks = entity.PackagePrograms?.Where(pp => !pp.IsDeleted).ToList() ?? new List<PackageProgram>();
                var existingProgramIds = existingProgramLinks.Select(x => x.ProgramId).ToHashSet();
                var requestedIds = request.ProgramIds != null ? request.ProgramIds.ToHashSet() : new HashSet<int>();

                var packageProgramRepo = _unitOfWork.Repository<PackageProgram>();

                // remove links that are not requested
                var toRemove = existingProgramLinks.Where(ep => !requestedIds.Contains(ep.ProgramId)).ToList();
                foreach (var ex in toRemove)
                {
                    packageProgramRepo.Delete(ex);
                }

                // add links that don't exist yet (handle soft-deleted ones by reviving)
                var allPackagePrograms = (await packageProgramRepo.GetAllAsync()).Where(pp => pp.PackageId == id).ToList();
                foreach (var pid in requestedIds.Where(pid => !existingProgramIds.Contains(pid)))
                {
                    var existingRow = allPackagePrograms.FirstOrDefault(pp => pp.ProgramId == pid);
                    if (existingRow != null)
                    {
                        if (existingRow.IsDeleted)
                        {
                            existingRow.IsDeleted = false;
                            packageProgramRepo.Update(existingRow);
                        }
                        // if exists and not deleted, nothing to do
                    }
                    else
                    {
                        packageProgramRepo.Add(new PackageProgram { PackageId = id, ProgramId = pid });
                    }
                }

                // update package entity
                _unitOfWork.Repository<Package>().Update(entity);
                await _unitOfWork.CompleteAsync();

                return true;
            }
            catch (DbUpdateException dbEx)
            {
                // wrap to provide clearer message for controller/middleware
                throw new InvalidOperationException("Database update conflict while updating package. Possible duplicate program link.", dbEx);
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var repo = _unitOfWork.Repository<Package>();
            var entity = await repo.GetByIdAsync(id);
            if (entity == null) return false;
            repo.Delete(entity);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> ToggleActiveAsync(int id)
        {
            var repo = _unitOfWork.Repository<Package>();
            var entity = await repo.GetByIdAsync(id);
            if (entity == null) return false;
            entity.IsActive = !entity.IsActive;
            repo.Update(entity);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<IReadOnlyList<PackageResponse>> GetAllAsync()
        {
            var repo = _unitOfWork.Repository<Package, IPackageRepository>();
            var list = (await repo.GetAllActiveWithProgramsAsync()).ToList();
            var mapped = list.Select(p => _mapper.Map<PackageResponse>(p)).ToList();

            for (int i = 0; i < mapped.Count; i++)
            {
                var pkgDto = mapped[i];
                var pkgEntity = list[i];
                if (!string.IsNullOrWhiteSpace(pkgDto.ThumbnailUrl))
                    pkgDto.ThumbnailUrl = _imageResolver.ResolveImageUrl(pkgDto.ThumbnailUrl);

                var programEntities = pkgEntity.PackagePrograms?.Where(pp => !pp.IsDeleted).Select(pp => pp.Program).Where(pr => pr != null).ToArray() ?? new Program[0];
                if (programEntities.Length > 0)
                {
                    pkgDto.Programs = programEntities.Select(pr => _mapper.Map<ProgramGetAllResponse>(pr)).ToArray();
                    pkgDto.ProgramIds = programEntities.Select(pr => pr.Id).ToArray();
                }
            }

            return mapped;
        }

        public async Task<IEnumerable<PackageWithSubscriptionsResponse>> GetPackagesWithSubscriptionsForTrainerAsync(int trainerProfileId)
        {
            var pkgRepo = _unitOfWork.Repository<Package, IPackageRepository>();
            var packages = (await pkgRepo.GetByTrainerIdAsync(trainerProfileId)).ToList();

            var subscriptionRepo = _unitOfWork.Repository<Subscription>();
            var result = new List<PackageWithSubscriptionsResponse>();

            foreach (var pkg in packages)
            {
                var spec = new SubscriptionsWithClientAndProgramSpecs(s => s.PackageId == pkg.Id && !s.IsDeleted);
                var subs = (await subscriptionRepo.GetAllWithSpecsAsync(spec)).ToList();

                var subsDto = subs.Select(s => new TrainerSubscriptionItem
                {
                    Id = s.Id,
                    PackageId = s.PackageId,
                    PackageName = s.Package?.Name,
                    ClientId = s.ClientId,
                    ClientName = s.Client?.FullName,
                    ClientEmail = s.Client?.Email,
                    Status = s.Status.ToString(),
                    StartDate = s.StartDate,
                    CurrentPeriodEnd = s.CurrentPeriodEnd,
                    AmountPaid = s.AmountPaid,
                    Currency = s.Currency,
                    SubscriptionType = s.IsAnnual ? "Annual" : "Monthly"
                }).ToList();
                var pkgDto = await GetByIdAsync(pkg.Id) ?? new PackageResponse
                {
                    Id = pkg.Id,
                    Name = pkg.Name,
                    Description = pkg.Description,
                    PriceMonthly = pkg.PriceMonthly,
                    PriceYearly = pkg.PriceYearly,
                    IsActive = pkg.IsActive,
                    ThumbnailUrl = pkg.ThumbnailUrl,
                    CreatedAt = pkg.CreatedAt
                };

                result.Add(new PackageWithSubscriptionsResponse
                {
                    Package = pkgDto,
                    Subscriptions = subsDto
                });
            }

            return result;
        }

        public async Task<IEnumerable<PackageWithProfitResponse>> GetPackagesWithProfitForTrainerAsync(int trainerProfileId)
        {
            // Use spec that includes subscriptions and their completed payments
            var pkgRepo = _unitOfWork.Repository<Package>();
            var packages = (await pkgRepo.GetAllWithSpecsAsync(new TrainerPackagesWithEarningsSpecs(trainerProfileId))).ToList();

            var result = new List<PackageWithProfitResponse>();

            foreach (var pkg in packages)
            {
                decimal profit = 0m;

                if (pkg.Subscriptions != null && pkg.Subscriptions.Any())
                {
                    foreach (var sub in pkg.Subscriptions)
                    {
                        // try to use payments if available
                        var completedPayments = sub.Payments?.Where(p => p.Status == PaymentStatus.Completed && !p.IsDeleted).ToList();
                        if (completedPayments != null && completedPayments.Count > 0)
                        {
                            profit += completedPayments.Sum(p => p.TrainerPayout);
                        }
                        else
                        {
                            // fallback: calculate trainer payout from subscription amount and platform fee percentage
                            var feePercent = sub.PlatformFeePercentage > 0 ? sub.PlatformFeePercentage : 15m;
                            var trainerPayout = sub.AmountPaid - (sub.AmountPaid * (feePercent / 100m));
                            profit += trainerPayout;
                        }
                    }
                }

                // compute subscription metrics
                var totalSubs = pkg.Subscriptions?.Count() ?? 0;
                var activeSubs = pkg.Subscriptions?.Count(s => s.Status == SubscriptionStatus.Active) ?? 0;
                var totalRevenue = pkg.Subscriptions?
                    .SelectMany(s => s.Payments ?? new List<Payment>())
                    .Where(p => p.Status == PaymentStatus.Completed && !p.IsDeleted)
                    .Sum(p => p.Amount) ?? 0m;

                // fallback revenue when Payments missing: sum AmountPaid from subscriptions
                if (totalRevenue == 0m)
                {
                    totalRevenue = pkg.Subscriptions?.Sum(s => s.AmountPaid) ?? 0m;
                }

                // platform fee assumed percentage from subscription (if varying take avg or compute per-sub)
                var platformFee = pkg.Subscriptions?.Sum(s => (s.PlatformFeePercentage / 100m) * s.AmountPaid) ?? 0m;

                result.Add(new PackageWithProfitResponse
                {
                    PackageId = pkg.Id,
                    Name = pkg.Name,
                    Description = pkg.Description,
                    PriceMonthly = pkg.PriceMonthly,
                    PriceYearly = pkg.PriceYearly,
                    Currency = pkg.Currency,
                    TrainerProfileId = pkg.Trainer?.Id,
                    TrainerUserId = pkg.Trainer?.UserId,
                    TrainerName = pkg.Trainer?.User?.FullName ?? pkg.Trainer?.Handle,
                    IsActive = pkg.IsActive,
                    CreatedAt = pkg.CreatedAt,
                    Profit = profit,
                    TotalSubscriptions = totalSubs,
                    ActiveSubscriptions = activeSubs,
                    TotalRevenue = totalRevenue,
                    PlatformFee = platformFee
                });
            }

            return result;
        }
    }
}