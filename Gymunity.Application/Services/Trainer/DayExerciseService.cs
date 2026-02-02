using AutoMapper;
using Gymunity.Application.Contracts.Services.Trainer;
using Gymunity.Application.DTOs.Program;
using Gymunity.Domain;
using Gymunity.Domain.Entities.ProgramAggregate;
using Gymunity.Domain.Interfaces.Trainer;

namespace Gymunity.Application.Services.Trainer
{
    public class DayExerciseService(IDayExerciseRepository repo, IUnitOfWork unitOfWork, IMapper mapper) : IDayExerciseService
    {
        private readonly IDayExerciseRepository _repo = repo;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<IReadOnlyList<ProgramDayExerciseGetAllResponse>> GetByDayAsync(int dayId)
        {
            var list = await _repo.GetByDayIdAsync(dayId);
            return list.Select(e => _mapper.Map<ProgramDayExerciseGetAllResponse>(e)).ToList();
        }

        public async Task<ProgramDayExerciseGetAllResponse?> GetByIdAsync(int id)
        {
            var e = await _repo.GetByIdAsync(id);
            if (e == null) return null;
            return _mapper.Map<ProgramDayExerciseGetAllResponse>(e);
        }

        public async Task<ProgramDayExerciseGetAllResponse> CreateAsync(ProgramDayExerciseGetAllResponse request)
        {
            var entity = new ProgramDayExercise
            {
                ProgramDayId = request.ProgramDayId,
                ExerciseId = request.ExerciseId,
                OrderIndex = request.OrderIndex,
                Sets = request.Sets,
                Reps = request.Reps,
                RestSeconds = request.RestSeconds,
                Tempo = request.Tempo,
                RPE = request.RPE,
                Percent1RM = request.Percent1RM,
                Notes = request.Notes,
                VideoUrl = request.VideoUrl,
                ExerciseDataJson = request.ExerciseDataJson
            };
            _repo.Add(entity);
            await _unitOfWork.CompleteAsync();
            return _mapper.Map<ProgramDayExerciseGetAllResponse>(entity);
        }

        public async Task<bool> UpdateAsync(int id, ProgramDayExerciseGetAllResponse request)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return false;
            entity.OrderIndex = request.OrderIndex;
            entity.Sets = request.Sets;
            entity.Reps = request.Reps;
            entity.RestSeconds = request.RestSeconds;
            entity.Tempo = request.Tempo;
            entity.RPE = request.RPE;
            entity.Percent1RM = request.Percent1RM;
            entity.Notes = request.Notes;
            entity.VideoUrl = request.VideoUrl;
            entity.ExerciseDataJson = request.ExerciseDataJson;
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
    }
}
