using AutoMapper;
using Gymunity.Application.Contracts.Services.Admin;
using Gymunity.Application.DTOs.Admin;
using Gymunity.Application.DTOs.Trainers;
using Gymunity.Domain;
using Gymunity.Domain.Entities.Trainer;
using Gymunity.Domain.Interfaces;

namespace Gymunity.Application.Services.Admin
{
    public class ReviewAdminService(IReviewAdminRepository repo, IUnitOfWork uow, IMapper mapper) : IReviewAdminService
    {
        private readonly IReviewAdminRepository _repo = repo;
        private readonly IUnitOfWork _uow = uow;
        private readonly IMapper _mapper = mapper;

        public async Task<AdminReviewActionResponse?> ApproveAsync(int reviewId)
        {
            var review = await _uow.Repository<TrainerReview>().GetByIdAsync(reviewId);
            if (review == null) return null;
            review.IsApproved = true;
            review.ApprovedAt = DateTimeOffset.UtcNow;
            _uow.Repository<TrainerReview>().Update(review);
            await _uow.CompleteAsync();
            var dto = _mapper.Map<AdminReviewActionResponse>(review);
            dto.Message = "Review approved";
            return dto;
        }

        public async Task<AdminReviewActionResponse?> RejectAsync(int reviewId)
        {
            var review = await _uow.Repository<TrainerReview>().GetByIdAsync(reviewId);
            if (review == null) return null;
            review.IsApproved = false;
            review.IsDeleted = true; // soft delete
            _uow.Repository<TrainerReview>().Update(review);
            await _uow.CompleteAsync();
            var dto = _mapper.Map<AdminReviewActionResponse>(review);
            dto.Message = "Review rejected and soft-deleted";
            return dto;
        }

        public async Task<bool> DeletePermanentAsync(int reviewId)
        {
            var review = await _uow.Repository<TrainerReview>().GetByIdAsync(reviewId);
            if (review == null) return false;
            _uow.Repository<TrainerReview>().Delete(review);
            await _uow.CompleteAsync();
            return true;
        }

        public async Task<IReadOnlyList<AdminReviewActionResponse>> GetPendingAsync()
        {
            var list = await _repo.GetAllPendingAsync();
            return list.Select(r => { var d = _mapper.Map<AdminReviewActionResponse>(r); d.Message = r.IsApproved ? "" : "Pending"; return d; }).ToList();
        }

        public async Task<IReadOnlyList<TrainerReviewResponse>> GetAllPendingAsync()
        {
            var list = await _repo.GetAllPendingAsync();
            return list.Select(r => _mapper.Map<TrainerReviewResponse>(r)).ToList();
        }
    }
}