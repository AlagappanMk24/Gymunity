using AutoMapper;
using Gymunity.Application.Contracts.Services;
using Gymunity.Application.DTOs.Client;
using Gymunity.Application.DTOs.Trainers;
using Gymunity.Application.Specifications;
using Gymunity.Application.Specifications.Trainer;
using Gymunity.Domain;
using Gymunity.Domain.Entities.ProgramAggregate;
using Gymunity.Domain.Entities.Trainer;
using Gymunity.Domain.Interfaces;
using Gymunity.Domain.Interfaces.Client;
using Gymunity.Domain.Interfaces.Trainer;
using ITI.Gymunity.FP.Application.DTOs.Client;
using DomainProgram = Gymunity.Domain.Entities.ProgramAggregate.Program;

namespace Gymunity.Application.Services
{
    public class HomeClientService(IUnitOfWork unitOfWork, IMapper mapper) : IHomeClientService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<(IReadOnlyList<ProgramClientResponse> programs, IReadOnlyList<TrainerClientResponse> trainers)> SearchAsync(string term)
        {
            var programSpec = new ProgramWithTrainerSpec(term);
            var programs = await _unitOfWork.Repository<DomainProgram>().GetAllWithSpecsAsync(programSpec);
            var programDtos = programs.Select(p => _mapper.Map<ProgramClientResponse>(p)).ToList();

            var trainerSpec = new TrainerWithUsersAndProgramsSpecs(tp => tp.Handle.Contains(term) || tp.User.FullName.Contains(term));
            var trainersList = (await _unitOfWork.Repository<TrainerProfile>().GetAllWithSpecsAsync(trainerSpec)).ToList();
            var trainerDtos = trainersList.Select(t => _mapper.Map<TrainerClientResponse>(t)).ToList();

            // populate reviews and summary for each trainer dto
            try
            {
                var reviewRepo = _unitOfWork.Repository<TrainerReview, IReviewClientRepository>();
                for (int i =0; i < trainersList.Count; i++)
                {
                    var profile = trainersList[i];
                    var dto = trainerDtos[i];

                    var reviews = (await reviewRepo.GetByTrainerIdAsync(profile.Id))
                        .Where(r => r.IsApproved && !r.IsDeleted)
                        .ToList();

                    dto.Reviews = reviews.Select(r => _mapper.Map<TrainerReviewResponse>(r)).ToList();
                    dto.TotalReviewsCount = dto.Reviews.Count;
                    dto.RatingSum = dto.Reviews.Sum(r => r.Rating);
                    dto.RatingAverageComputed = dto.TotalReviewsCount >0 ? Math.Round((decimal)dto.RatingSum / dto.TotalReviewsCount,2) :0;
                }
            }
            catch
            {
                // ignore and continue with empty review data
                foreach (var dto in trainerDtos)
                {
                    dto.Reviews = new List<TrainerReviewResponse>();
                    dto.TotalReviewsCount =0;
                    dto.RatingSum =0;
                    dto.RatingAverageComputed =0;
                }
            }

            return (programDtos, trainerDtos);
        }

        public async Task<IReadOnlyList<ProgramClientResponse>> GetAllProgramsAsync()
        {
            var spec = new ProgramWithTrainerSpec();
            var programs = await _unitOfWork.Repository<DomainProgram>().GetAllWithSpecsAsync(spec);
            return programs.Select(p => _mapper.Map<ProgramClientResponse>(p)).ToList();
        }

        public async Task<ProgramClientResponse?> GetProgramByIdAsync(int id)
        {
            // ProgramWithTrainerSpec only accepts optional searchTerm string in constructor.
            // Create default spec and set Criteria to filter by id.
            var specById = new ProgramWithTrainerSpec();
            specById.Criteria = p => p.Id == id;
            var program = (await _unitOfWork.Repository<DomainProgram>().GetAllWithSpecsAsync(specById)).FirstOrDefault();
            if (program == null) return null;
            return _mapper.Map<ProgramClientResponse>(program);
        }

        public async Task<IReadOnlyList<TrainerClientResponse>> GetAllTrainersAsync()
        {
            var spec = new TrainerWithUsersAndProgramsSpecs();
            var trainersList = (await _unitOfWork.Repository<TrainerProfile>().GetAllWithSpecsAsync(spec)).ToList();
            var trainerDtos = trainersList.Select(t => _mapper.Map<TrainerClientResponse>(t)).ToList();

            try
            {
                var reviewRepo = _unitOfWork.Repository<TrainerReview, IReviewClientRepository>();
                for (int i =0; i < trainersList.Count; i++)
                {
                    var profile = trainersList[i];
                    var dto = trainerDtos[i];

                    var reviews = (await reviewRepo.GetByTrainerIdAsync(profile.Id))
                        .Where(r => r.IsApproved && !r.IsDeleted)
                        .ToList();

                    dto.Reviews = reviews.Select(r => _mapper.Map<TrainerReviewResponse>(r)).ToList();
                    dto.TotalReviewsCount = dto.Reviews.Count;
                    dto.RatingSum = dto.Reviews.Sum(r => r.Rating);
                    dto.RatingAverageComputed = dto.TotalReviewsCount >0 ? Math.Round((decimal)dto.RatingSum / dto.TotalReviewsCount,2) :0;
                }
            }
            catch
            {
                foreach (var dto in trainerDtos)
                {
                    dto.Reviews = new List<TrainerReviewResponse>();
                    dto.TotalReviewsCount =0;
                    dto.RatingSum =0;
                    dto.RatingAverageComputed =0;
                }
            }

            return trainerDtos;
        }

        public async Task<TrainerClientResponse?> GetTrainerByIdAsync(int id)
        {
            var spec = new TrainerWithUsersAndProgramsSpecs(tp => tp.Id == id);
            var trainer = await _unitOfWork.Repository<TrainerProfile>().GetWithSpecsAsync(spec);
            if (trainer == null) return null;

            var dto = _mapper.Map<TrainerClientResponse>(trainer);

            // fetch approved, non-deleted reviews for this trainer and populate summary
            try
            {
                var reviewRepo = _unitOfWork.Repository<TrainerReview, IReviewClientRepository>();
                var reviews = (await reviewRepo.GetByTrainerIdAsync(id))
                    .Where(r => r.IsApproved && !r.IsDeleted)
                    .ToList();

                dto.Reviews = reviews.Select(r => _mapper.Map<TrainerReviewResponse>(r)).ToList();
                dto.TotalReviewsCount = dto.Reviews.Count;
                dto.RatingSum = dto.Reviews.Sum(r => r.Rating);
                dto.RatingAverageComputed = dto.TotalReviewsCount >0 ? Math.Round((decimal)dto.RatingSum / dto.TotalReviewsCount,2) :0;
            }
            catch
            {
                // If review repository not available or fails, leave reviews empty and continue
                dto.Reviews = new List<TrainerReviewResponse>();
                dto.TotalReviewsCount = 0;
                dto.RatingSum = 0;
                dto.RatingAverageComputed = 0;
            }

            return dto;
        }

        // packages
        public async Task<IReadOnlyList<PackageClientResponse>> GetAllPackagesAsync()
        {
            var repo = _unitOfWork.Repository<Package, IPackageRepository>();
            var list = (await repo.GetAllActiveWithProgramsAsync()).ToList();
            var mapped = list.Select(p => _mapper.Map<PackageClientResponse>(p)).ToList();

            for (int i =0; i < mapped.Count; i++)
            {
                var pkg = mapped[i];
                var original = list[i];

                if (!string.IsNullOrWhiteSpace(pkg.ThumbnailUrl))
                    pkg.ThumbnailUrl = _unitOfWork.Repository<DomainProgram>().GetType() != null ? pkg.ThumbnailUrl : pkg.ThumbnailUrl; // preserve existing behavior

                var programEntities = original.PackagePrograms?.Where(pp => !pp.IsDeleted).Select(pp => pp.Program).Where(pr => pr != null).ToArray() ?? Array.Empty<DomainProgram>();

                pkg.Programs = programEntities.Select(pr => new ProgramBriefResponse
                {
                    Title = pr.Title,
                    Description = pr.Description,
                    ThumbnailUrl = pr.ThumbnailUrl,
                    Type = pr.Type,
                    DurationWeeks = pr.DurationWeeks
                }).ToArray();

                // ensure non-null array
                if (pkg.Programs == null) pkg.Programs = new ProgramBriefResponse[0];
            }

            return mapped;
        }

        public async Task<PackageClientResponse?> GetPackageByIdAsync(int id)
        {
            var repo = _unitOfWork.Repository<Package, IPackageRepository>();
            var p = await repo.GetByIdWithProgramsAsync(id);
            if (p == null) return null;
            var mapped = _mapper.Map<PackageClientResponse>(p);

            var programEntities = p.PackagePrograms?.Where(pp => !pp.IsDeleted).Select(pp => pp.Program).Where(pr => pr != null).ToArray() ?? new DomainProgram[0];

            mapped.Programs = programEntities.Select(pr => new ProgramBriefResponse
            {
                Title = pr.Title,
                Description = pr.Description,
                ThumbnailUrl = pr.ThumbnailUrl,
                Type = pr.Type,
                DurationWeeks = pr.DurationWeeks
            }).ToArray();

            if (mapped.Programs == null) mapped.Programs = new ProgramBriefResponse[0];

            return mapped;
        }

        public async Task<IReadOnlyList<PackageClientResponse>> GetPackagesByTrainerIdAsync(int trainerProfileId)
        {
            var repo = _unitOfWork.Repository<Package, IPackageRepository>();
            var list = (await repo.GetByTrainerIdAsync(trainerProfileId)).ToList();
            var mapped = list.Select(p => _mapper.Map<PackageClientResponse>(p)).ToList();

            for (int i =0; i < mapped.Count; i++)
            {
                var pkg = mapped[i];
                var original = list[i];
                var programEntities = original.PackagePrograms?.Where(pp => !pp.IsDeleted).Select(pp => pp.Program).Where(pr => pr != null).ToArray() ?? new DomainProgram[0];

                pkg.Programs = programEntities.Select(pr => new ProgramBriefResponse
                {
                    Title = pr.Title,
                    Description = pr.Description,
                    ThumbnailUrl = pr.ThumbnailUrl,
                    Type = pr.Type,
                    DurationWeeks = pr.DurationWeeks
                }).ToArray();

                if (pkg.Programs == null) pkg.Programs = new ProgramBriefResponse[0];
            }

            return mapped;
        }

        // Program client endpoints
        public async Task<IReadOnlyList<ProgramClientResponse>> GetProgramsByTrainerIdAsync(string trainerId)
        {
            try
            {
                var repo = _unitOfWork.Repository<DomainProgram, IProgramRepository>();
                var list = await repo.GetByTrainerAsync(trainerId);
                return list.Select(p => _mapper.Map<ProgramClientResponse>(p)).ToList();
            }
            catch
            {
                var all = await _unitOfWork.Repository<DomainProgram>().GetAllAsync();
                var byTrainer = all.Where(p => p.TrainerProfile != null && p.TrainerProfile.UserId == trainerId).ToList();
                return byTrainer.Select(p => _mapper.Map<ProgramClientResponse>(p)).ToList();
            }
        }

        public async Task<IReadOnlyList<PackageClientResponse>> GetPackagesByTrainerAsync(int trainerUserId)
        {
            var repo = _unitOfWork.Repository<Package, IPackageRepository>();
            var list = (await repo.GetAllActiveWithProgramsAsync()).Where(p => p.TrainerId == trainerUserId).ToList();
            var mapped = list.Select(p => _mapper.Map<PackageClientResponse>(p)).ToList();

            for (int i =0; i < mapped.Count; i++)
            {
                var pkg = mapped[i];
                var original = list[i];
                var programEntities = original.PackagePrograms?.Where(pp => !pp.IsDeleted).Select(pp => pp.Program).Where(pr => pr != null).ToArray() ?? new DomainProgram[0];

                pkg.Programs = [.. programEntities.Select(pr => new ProgramBriefResponse
                {
                    Title = pr.Title,
                    Description = pr.Description,
                    ThumbnailUrl = pr.ThumbnailUrl,
                    Type = pr.Type,
                    DurationWeeks = pr.DurationWeeks
                })];

                if (pkg.Programs == null) pkg.Programs = [];
            }

            return mapped;
        }

        public async Task<IReadOnlyList<ProgramClientResponse>> GetProgramsByTrainerProfileIdAsync(int trainerProfileId)
        {
            var trainer = await _unitOfWork.Repository<TrainerProfile>().GetByIdAsync(trainerProfileId);
            if (trainer == null) return new List<ProgramClientResponse>();
            var all = await _unitOfWork.Repository<DomainProgram>().GetAllAsync();
            var byTrainer = all.Where(p => p.TrainerProfileId == trainer.Id).ToList();
            return byTrainer.Select(p => _mapper.Map<ProgramClientResponse>(p)).ToList();
        }
    }
}