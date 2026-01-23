namespace Gymunity.Application.DTOs.Client
{
    public class ClientSubscribeProgramRequest
    {
        public string ClientId { get; set; } = null!;
        public int ProgramId { get; set; }
        public int? PackageId { get; set; }
    }
}
