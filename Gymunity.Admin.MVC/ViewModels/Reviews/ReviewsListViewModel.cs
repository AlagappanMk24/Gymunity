using Gymunity.Application.DTOs.Trainers;

namespace Gymunity.Admin.MVC.ViewModels.Reviews
{
    public class ReviewsListViewModel
    {
        public List<TrainerReviewResponse> Reviews { get; set; } = [];
        
        public int PageNumber { get; set; } = 1;
        
        public int PageSize { get; set; } = 10;
        
        public int TotalCount { get; set; } = 0;

        /// <summary>
        /// Total count of pending reviews across all pages
        /// </summary>
        public int PendingCount { get; set; } = 0;

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

        /// <summary>
        /// Get average rating from displayed reviews
        /// </summary>
        public double AverageRating => Reviews.Count > 0 
            ? Reviews.Average(r => r.Rating) 
            : 0;
    }
}