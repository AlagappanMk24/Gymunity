namespace Gymunity.Application.Contracts.ExternalServices.RealTime
{
    /// <summary>
    /// Interface for managing SignalR connections and notifications
    /// </summary>
    public interface ISignalRConnectionManager
    {
        void AddConnection(string userId, string connectionId);
        void RemoveConnection(string connectionId);
        string? GetConnectionId(string userId);
        IEnumerable<string> GetAllConnections(string userId);
        void AddUserToGroup(string userId, string groupName);
        void RemoveUserFromGroup(string userId, string groupName);
    }
}