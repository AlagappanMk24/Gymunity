using Gymunity.Application.DTOs.Trainers;

namespace Gymunity.Admin.MVC.ViewModels.Trainers
{
    public class TrainersListViewModel
    {
        public List<TrainerProfileDetailResponse> Trainers { get; set; } = new List<TrainerProfileDetailResponse>();
        
        public int PageNumber { get; set; } = 1;
        
        public int PageSize { get; set; } = 10;
        
        public int TotalCount { get; set; } = 0;
        
        public string? SearchTerm { get; set; }
        
        public bool? IsVerifiedFilter { get; set; }
        
        public bool? IsSuspendedFilter { get; set; }

        /// <summary>
        /// Total number of pages
        /// </summary>
        public int TotalPages => (TotalCount + PageSize - 1) / PageSize;

        /// <summary>
        /// Check if there are more pages
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;

        /// <summary>
        /// Check if there are previous pages
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;
    }
}