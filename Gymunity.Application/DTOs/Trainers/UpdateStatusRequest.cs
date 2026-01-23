using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Gymunity.Application.DTOs.Trainers
{
    public class UpdateStatusRequest
    {
        public IFormFile? StatusImage { get; set; }

        [StringLength(200)]
        public string? StatusDescription { get; set; }
    }
}