namespace Gymunity.Application.Contracts.Services.Communication
{
    public interface IChatBotService
    {
        Task<string> AskAboutPackagesAsync(string question);
        Task<string> AskAsync(string prompt, string? modelName = null, bool requireJsonOnly = false);
    }
}
