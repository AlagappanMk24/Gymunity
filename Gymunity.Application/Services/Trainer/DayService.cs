using AutoMapper;
using Gymunity.Application.Contracts.Services.Trainer;
using Gymunity.Application.DTOs.Program;
using Gymunity.Domain;
using Gymunity.Domain.Entities.ProgramAggregate;
using Gymunity.Domain.Interfaces.Trainer;

namespace Gymunity.Application.Services.Trainer
{
    public class DayService(IDayRepository repo, IUnitOfWork unitOfWork, IMapper mapper) : IDayService
    {
        private readonly IDayRepository _repo = repo;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<IReadOnlyList<ProgramDayGetAllResponse>> GetByWeekAsync(int weekId)
        {
            var list = await _repo.GetByWeekIdAsync(weekId);
            return list.Select(d => _mapper.Map<ProgramDayGetAllResponse>(d)).ToList();
        }

        public async Task<ProgramDayGetAllResponse?> GetByIdAsync(int id)
        {
            var d = await _repo.GetWithExercisesAsync(id);
            if (d == null) return null;
            return _mapper.Map<ProgramDayGetAllResponse>(d);
        }

        public async Task<ProgramDayGetAllResponse> CreateAsync(ProgramDayGetAllResponse request)
        {
            var entity = new ProgramDay { ProgramWeekId = request.ProgramWeekId, DayNumber = request.DayNumber, Title = request.Title, Notes = request.Notes };
            _repo.Add(entity);
            await _unitOfWork.CompleteAsync();
            return _mapper.Map<ProgramDayGetAllResponse>(entity);
        }

        public async Task<bool> UpdateAsync(int id, ProgramDayGetAllResponse request)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return false;
            entity.DayNumber = request.DayNumber;
            entity.Title = request.Title;
            entity.Notes = request.Notes;
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
