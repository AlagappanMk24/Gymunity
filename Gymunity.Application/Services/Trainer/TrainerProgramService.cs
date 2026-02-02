using AutoMapper;
using Gymunity.Application.Contracts.Services.Trainer;
using Gymunity.Application.DTOs.Program;
using Gymunity.Domain;
using Gymunity.Domain.Entities.ProgramAggregate;
using Gymunity.Domain.Interfaces;

namespace Gymunity.Application.Services.Trainer
{
    public class TrainerProgramService(IProgramRepository repo, IUnitOfWork unitOfWork, IMapper mapper, ITrainerProfileRepository trainerRepo) : ITrainerProgramService
    {
        private readonly IProgramRepository _repo = repo;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly ITrainerProfileRepository _trainerRepo = trainerRepo;

        public async Task<IReadOnlyList<ProgramGetAllResponse>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return list.Select(p => _mapper.Map<ProgramGetAllResponse>(p)).ToList();
        }

        public async Task<ProgramGetByIdResponse?> GetByIdAsync(int id)
        {
            var p = await _repo.GetByIdWithIncludesAsync(id);
            if (p == null) return null;
            return _mapper.Map<ProgramGetByIdResponse>(p);
        }

        public async Task<ProgramGetByIdResponse> CreateAsync(ProgramCreateRequest request)
        {
            // Validate trainer profile exists
            var profile = await _trainerRepo.GetByIdAsync(request.TrainerProfileId) ?? throw new InvalidOperationException($"Trainer profile with id {request.TrainerProfileId} not found.");

            // Global duplicate title check using DB-side query
            if (!string.IsNullOrWhiteSpace(request.Title))
            {
                if (await _repo.ExistsByTitleAsync(request.Title.Trim()))
                {
                    throw new InvalidOperationException($"A program with title '{request.Title}' already exists.");
                }
            }

            // Ensure we have a non-null trainer user id for legacy column
            var trainerUserId = profile.UserId ?? string.Empty;

            var entity = new Program
            {
                TrainerProfileId = request.TrainerProfileId,
                TrainerProfile = profile,
                TrainerId = trainerUserId,
                Title = request.Title,
                Description = request.Description,
                Type = request.Type,
                DurationWeeks = request.DurationWeeks,
                Price = request.Price,
                IsPublic = request.IsPublic,
                MaxClients = request.MaxClients,
                ThumbnailUrl = request.ThumbnailUrl,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _repo.Add(entity);
            await _unitOfWork.CompleteAsync();
            return _mapper.Map<ProgramGetByIdResponse>(entity);
        }

        public async Task<bool> UpdateAsync(int id, ProgramUpdateRequest request)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return false;
            entity.Title = request.Title;
            entity.Description = request.Description;
            entity.Type = request.Type;
            entity.DurationWeeks = request.DurationWeeks;
            entity.Price = request.Price;
            entity.IsPublic = request.IsPublic;
            entity.MaxClients = request.MaxClients;
            entity.ThumbnailUrl = request.ThumbnailUrl;
            entity.UpdatedAt = DateTime.UtcNow;

            // if TrainerProfileId changed in future requests, update legacy TrainerId accordingly (not part of current DTO)
            _repo.Update(entity);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return false;
            _repo.Delete(entity);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<IReadOnlyList<ProgramGetAllResponse>> SearchAsync(string? term)
        {
            var list = await _repo.SearchAsync(term);
            return list.Select(p => _mapper.Map<ProgramGetAllResponse>(p)).ToList();
        }

        public async Task<IReadOnlyList<ProgramGetAllResponse>> GetByTrainerAsync(string trainerId)
        {
            if (int.TryParse(trainerId, out var profileId))
            {
                var list = await _repo.GetByTrainerAsyncProfileId(profileId);
                return list.Select(p => _mapper.Map<ProgramGetAllResponse>(p)).ToList();
            }
            else
            {
                var list = await _repo.GetByTrainerAsync(trainerId);
                return list.Select(p => _mapper.Map<ProgramGetAllResponse>(p)).ToList();
            }
        }
    }
}