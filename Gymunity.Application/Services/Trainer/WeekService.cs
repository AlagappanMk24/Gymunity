using AutoMapper;
using Gymunity.Application.Contracts.Services.Trainer;
using Gymunity.Application.DTOs.Program;
using Gymunity.Domain;
using Gymunity.Domain.Entities.ProgramAggregate;
using Gymunity.Domain.Interfaces.Trainer;

namespace Gymunity.Application.Services.Trainer
{
    public class WeekService(IWeekRepository repo, IUnitOfWork unitOfWork, IMapper mapper) : IWeekService
    {
        private readonly IWeekRepository _repo = repo;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<IReadOnlyList<ProgramWeekGetAllResponse>> GetByProgramAsync(int programId)
        {
            var list = await _repo.GetByProgramIdAsync(programId);
            return list.Select(w => _mapper.Map<ProgramWeekGetAllResponse>(w)).ToList();
        }
        public async Task<ProgramWeekGetAllResponse?> GetByIdAsync(int id)
        {
            var w = await _repo.GetWithDaysAsync(id);
            if (w == null) return null;
            return _mapper.Map<ProgramWeekGetAllResponse>(w);
        }
        public async Task<ProgramWeekGetAllResponse> CreateAsync(ProgramWeekGetAllResponse request)
        {
            var entity = new ProgramWeek { ProgramId = request.ProgramId, WeekNumber = request.WeekNumber };
            _repo.Add(entity);
            await _unitOfWork.CompleteAsync();
            return _mapper.Map<ProgramWeekGetAllResponse>(entity);
        }
        public async Task<bool> UpdateAsync(int id, ProgramWeekGetAllResponse request)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return false;
            entity.WeekNumber = request.WeekNumber;
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