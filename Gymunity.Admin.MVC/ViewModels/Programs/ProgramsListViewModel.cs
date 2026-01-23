using Gymunity.Application.DTOs.Program;

namespace Gymunity.Admin.MVC.ViewModels.Programs
{
    public class ProgramsListViewModel
    {
        public List<ProgramGetAllResponse> Programs { get; set; } = new List<ProgramGetAllResponse>();
        
        public int PageNumber { get; set; } = 1;
        
        public int PageSize { get; set; } = 10;
        
        public int TotalCount { get; set; } = 0;
        
        public string? SearchTerm { get; set; }
        
        public bool? IsPublicFilter { get; set; }
        
        public string? ProgramTypeFilter { get; set; }
        
        public int? TrainerFilterId { get; set; }

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