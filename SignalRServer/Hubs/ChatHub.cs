using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace SignalRServer.Hubs
{
    public class ChatHub : Hub
    {
        private static readonly ConcurrentDictionary<string, UserConnection> _connections = new();
        private static readonly ConcurrentDictionary<string, HashSet<string>> _groups = new();
        private static readonly object _lockObject = new();

        public override async Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            var userAgent = Context.GetHttpContext()?.Request.Headers["User-Agent"].ToString();
            
            _connections.TryAdd(connectionId, new UserConnection
            {
                ConnectionId = connectionId,
                ConnectedAt = DateTime.UtcNow,
                UserAgent = userAgent
            });

            // Send connection info to all clients
            await Clients.All.SendAsync("UserConnected", connectionId, _connections.Count);
            
            Console.WriteLine($"User connected: {connectionId}, Total connections: {_connections.Count}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            
            _connections.TryRemove(connectionId, out var userConnection);

            // Remove from all groups
            lock (_lockObject)
            {
                foreach (var group in _groups.Values)
                {
                    group.Remove(connectionId);
                }
            }

            // Send disconnection info to all clients
            await Clients.All.SendAsync("UserDisconnected", connectionId, _connections.Count);
            
            Console.WriteLine($"User disconnected: {connectionId}, Total connections: {_connections.Count}");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string message)
        {
            var connectionId = Context.ConnectionId;
            var timestamp = DateTime.UtcNow;
            
            await Clients.All.SendAsync("ReceiveMessage", connectionId, message, timestamp);
        }

        public async Task SendMessageToUser(string targetConnectionId, string message)
        {
            var connectionId = Context.ConnectionId;
            var timestamp = DateTime.UtcNow;
            
            await Clients.Client(targetConnectionId).SendAsync("ReceivePrivateMessage", connectionId, message, timestamp);
        }

        public async Task JoinGroup(string groupName)
        {
            var connectionId = Context.ConnectionId;
            
            await Groups.AddToGroupAsync(connectionId, groupName);
            
            lock (_lockObject)
            {
                if (!_groups.ContainsKey(groupName))
                {
                    _groups[groupName] = new HashSet<string>();
                }
                _groups[groupName].Add(connectionId);
            }

            await Clients.Group(groupName).SendAsync("UserJoinedGroup", connectionId, groupName);
        }

        public async Task LeaveGroup(string groupName)
        {
            var connectionId = Context.ConnectionId;
            
            await Groups.RemoveFromGroupAsync(connectionId, groupName);
            
            lock (_lockObject)
            {
                if (_groups.ContainsKey(groupName))
                {
                    _groups[groupName].Remove(connectionId);
                    if (_groups[groupName].Count == 0)
                    {
                        _groups.TryRemove(groupName, out _);
                    }
                }
            }

            await Clients.Group(groupName).SendAsync("UserLeftGroup", connectionId, groupName);
        }

        public async Task SendMessageToGroup(string groupName, string message)
        {
            var connectionId = Context.ConnectionId;
            var timestamp = DateTime.UtcNow;
            
            await Clients.Group(groupName).SendAsync("ReceiveGroupMessage", connectionId, groupName, message, timestamp);
        }

        public async Task Echo(string message)
        {
            var connectionId = Context.ConnectionId;
            var timestamp = DateTime.UtcNow;
            
            await Clients.Caller.SendAsync("EchoResponse", message, timestamp);
        }

        public async Task Broadcast(string message)
        {
            var connectionId = Context.ConnectionId;
            var timestamp = DateTime.UtcNow;
            
            await Clients.All.SendAsync("BroadcastMessage", connectionId, message, timestamp);
        }

        public async Task GetConnectionCount()
        {
            await Clients.Caller.SendAsync("ConnectionCount", _connections.Count);
        }

        public async Task GetConnectionInfo()
        {
            var connectionId = Context.ConnectionId;
            if (_connections.TryGetValue(connectionId, out var userConnection))
            {
                await Clients.Caller.SendAsync("ConnectionInfo", userConnection);
            }
        }

        public async Task GetGroupInfo()
        {
            var groupInfo = new Dictionary<string, int>();
            
            lock (_lockObject)
            {
                foreach (var group in _groups)
                {
                    groupInfo[group.Key] = group.Value.Count;
                }
            }

            await Clients.Caller.SendAsync("GroupInfo", groupInfo);
        }

        // Performance testing methods
        public async Task SendBulkMessages(int count)
        {
            var connectionId = Context.ConnectionId;
            var timestamp = DateTime.UtcNow;
            
            for (int i = 0; i < count; i++)
            {
                await Clients.Caller.SendAsync("BulkMessage", i, $"Message {i}", timestamp);
            }
        }

        public async Task SendLargeMessage(int sizeKb)
        {
            var connectionId = Context.ConnectionId;
            var timestamp = DateTime.UtcNow;
            var message = new string('A', sizeKb * 1024);
            
            await Clients.Caller.SendAsync("LargeMessage", message, sizeKb, timestamp);
        }

        // Stress testing method
        public async Task StressTest(int messageCount, int delayMs)
        {
            var connectionId = Context.ConnectionId;
            
            for (int i = 0; i < messageCount; i++)
            {
                await Clients.Caller.SendAsync("StressTestMessage", i, $"Stress message {i}", DateTime.UtcNow);
                
                if (delayMs > 0)
                {
                    await Task.Delay(delayMs);
                }
            }
        }
    }

    public class UserConnection
    {
        public string ConnectionId { get; set; } = string.Empty;
        public DateTime ConnectedAt { get; set; }
        public string? UserAgent { get; set; }
    }
}