using Gymunity.Application.DTOs.ClientDto;

namespace Gymunity.Admin.MVC.ViewModels.Clients
{
    public class ClientsListViewModel
    {
        public List<ClientListItemDto> Clients { get; set; } = new List<ClientListItemDto>();
        
        public int PageNumber { get; set; } = 1;
        
        public int PageSize { get; set; } = 10;
        
        public int TotalCount { get; set; } = 0;
        
        public string? SearchTerm { get; set; }
        
        public bool? IsActiveFilter { get; set; }

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