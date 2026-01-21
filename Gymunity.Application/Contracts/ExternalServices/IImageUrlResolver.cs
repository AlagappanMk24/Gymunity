namespace Gymunity.Application.Contracts.ExternalServices
{
    public interface IImageUrlResolver
    {
        string? ResolveImageUrl(string url);
    }
}