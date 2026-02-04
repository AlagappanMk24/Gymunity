using Gymunity.Application.Contracts.ExternalServices.RealTime;
using System.Collections.Concurrent;

namespace Gymunity.Infrastructure.ExternalServices
{
    /// <summary>
    /// Manages SignalR connections for real-time communication
    /// Supports multiple connections per user (e.g., multiple tabs/devices)
    /// Uses thread-safe ConcurrentDictionary for optimal performance under high concurrency
    /// </summary>
    public class SignalRConnectionManager : ISignalRConnectionManager
    {
        // ConcurrentDictionary provides thread-safe operations without manual locking
        private static readonly ConcurrentDictionary<string, List<string>> _connections = new();
        private static readonly ConcurrentDictionary<string, HashSet<string>> _userGroups = new();

        public void AddConnection(string userId, string connectionId)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(connectionId))
                return;

            // GetOrAdd is atomic - creates new list if key doesn't exist
            var connectionsList = _connections.GetOrAdd(userId, _ => new List<string>());
            
            // Lock the list itself for thread-safe collection operations
            lock (connectionsList)
            {
                if (!connectionsList.Contains(connectionId))
                    connectionsList.Add(connectionId);
            }
        }

        public void RemoveConnection(string connectionId)
        {
            // Find the user with this connection
            var userEntry = _connections.FirstOrDefault(x => x.Value.Contains(connectionId));
            
            if (string.IsNullOrEmpty(userEntry.Key))
                return;

            var userId = userEntry.Key;
            
            if (_connections.TryGetValue(userId, out var connectionsList))
            {
                lock (connectionsList)
                {
                    connectionsList.Remove(connectionId);
                    
                    // Remove user if no more connections
                    if (connectionsList.Count == 0)
                        _connections.TryRemove(userId, out _);
                }
            }
        }

        public string? GetConnectionId(string userId)
        {
            if (_connections.TryGetValue(userId, out var connections))
            {
                lock (connections)
                {
                    return connections.FirstOrDefault();
                }
            }
            return null;
        }

        public IEnumerable<string> GetAllConnections(string userId)
        {
            if (_connections.TryGetValue(userId, out var connections))
            {
                lock (connections)
                {
                    return connections.ToList();
                }
            }
            return Enumerable.Empty<string>();
        }

        public void AddUserToGroup(string userId, string groupName)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(groupName))
                return;

            // GetOrAdd is atomic - creates new HashSet if key doesn't exist
            var groupUsers = _userGroups.GetOrAdd(groupName, _ => new HashSet<string>());
            
            // Lock the HashSet itself for thread-safe collection operations
            lock (groupUsers)
            {
                groupUsers.Add(userId);
            }
        }

        public void RemoveUserFromGroup(string userId, string groupName)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(groupName))
                return;

            if (_userGroups.TryGetValue(groupName, out var users))
            {
                lock (users)
                {
                    users.Remove(userId);
                    
                    // Remove group if no more users
                    if (users.Count == 0)
                        _userGroups.TryRemove(groupName, out _);
                }
            }
        }
    }
}