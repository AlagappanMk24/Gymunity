using Gymunity.Application.Contracts.ExternalServices;
using Microsoft.Extensions.Configuration;

namespace Gymunity.Infrastructure.Services
{
    public class ImageUrlResolver(IConfiguration configuration): IImageUrlResolver
    {
        private readonly IConfiguration _configuration = configuration;
        public string? ResolveImageUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return null;

            var baseUrl = _configuration["BaseApiUrl"];
            if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                return url;
            }
            else
            {
                return $"{baseUrl?.TrimEnd('/')}/{url.TrimStart('/')}";
            }
        }
    }
}