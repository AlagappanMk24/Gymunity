using System.ComponentModel.DataAnnotations;

namespace Gymunity.Application.DTOs.Packages
{
    public class PackageUpdateRequest
    {
        [StringLength(256)]
        public string? Name { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        public decimal? PriceMonthly { get; set; }

        public decimal? PriceYearly { get; set; }

        public bool? IsActive { get; set; }

        [StringLength(500)]
        public string? ThumbnailUrl { get; set; }

        public int[]? ProgramIds { get; set; }
    }
}