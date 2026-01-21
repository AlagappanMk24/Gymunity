using Gymunity.Application.Contracts.ExternalServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Gymunity.Infrastructure.ExternalServices
{
    public class FileUploadService(IWebHostEnvironment environment) : IFileUploadService
    {
        private readonly IWebHostEnvironment _environment = environment;
        private readonly string[] _allowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB

        public async Task<string> UploadImageAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                return null!;

            if (!IsValidImageFile(file))
                throw new InvalidOperationException("Invalid image file format or size.");

            // Create unique filename
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            // Create upload directory if it doesn't exist
            var uploadPath = Path.Combine(_environment.WebRootPath, "images", folder);
            Directory.CreateDirectory(uploadPath);

            // Full file path
            var filePath = Path.Combine(uploadPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative path for database storage
            return Path.Combine("images", folder, fileName).Replace("\\", "/");
        }

        public bool DeleteImage(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;


            var fullPath = Path.Combine(_environment.WebRootPath, filePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return true;
            }
            return false;
        }

        public bool IsValidImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            if (file.Length > _maxFileSize)
                return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return _allowedImageExtensions.Contains(extension);
        }
    }
}
