using Microsoft.AspNetCore.Http;

namespace Gymunity.Application.Contracts.ExternalServices
{
    public interface IFileUploadService
    {
        const string UserProfilePhotosFolder = "profile-photos";
        Task<string> UploadImageAsync(IFormFile file, string folder);
        bool DeleteImage(string filePath);
        bool IsValidImageFile(IFormFile file);
    }
}