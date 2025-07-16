using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace SignalRServer.Hubs;

public class ChatHub : Hub
{
    private static readonly ConcurrentDictionary<string, string> _connections = new();
    private static readonly ConcurrentDictionary<string, HashSet<string>> _groups = new();
    private static readonly object _lock = new();

    // Connection Events
    public override async Task OnConnectedAsync()
    {
        _connections.TryAdd(Context.ConnectionId, Context.ConnectionId);
        await Clients.All.SendAsync("UserConnected", Context.ConnectionId, _connections.Count);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _connections.TryRemove(Context.ConnectionId, out _);
        
        // Remove from all groups
        lock (_lock)
        {
            foreach (var group in _groups.Values)
            {
                group.Remove(Context.ConnectionId);
            }
        }

        await Clients.All.SendAsync("UserDisconnected", Context.ConnectionId, _connections.Count);
        await base.OnDisconnectedAsync(exception);
    }

    // Basic Messaging Methods
    public async Task SendMessage(string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", Context.ConnectionId, message);
    }

    public async Task SendMessageToUser(string targetConnectionId, string message)
    {
        await Clients.Client(targetConnectionId).SendAsync("ReceivePrivateMessage", Context.ConnectionId, message);
    }

    public async Task Echo(string message)
    {
        await Clients.Caller.SendAsync("EchoResponse", message, DateTime.UtcNow.ToString("O"));
    }

    public async Task Broadcast(string message)
    {
        await Clients.All.SendAsync("BroadcastMessage", Context.ConnectionId, message, DateTime.UtcNow);
    }

    // Group Methods
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        
        lock (_lock)
        {
            if (!_groups.ContainsKey(groupName))
            {
                _groups[groupName] = new HashSet<string>();
            }
            _groups[groupName].Add(Context.ConnectionId);
        }

        await Clients.Group(groupName).SendAsync("UserJoinedGroup", Context.ConnectionId, groupName);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        
        lock (_lock)
        {
            if (_groups.ContainsKey(groupName))
            {
                _groups[groupName].Remove(Context.ConnectionId);
                if (_groups[groupName].Count == 0)
                {
                    _groups.TryRemove(groupName, out _);
                }
            }
        }

        await Clients.Group(groupName).SendAsync("UserLeftGroup", Context.ConnectionId, groupName);
    }

    public async Task SendMessageToGroup(string groupName, string message)
    {
        await Clients.Group(groupName).SendAsync("ReceiveGroupMessage", Context.ConnectionId, groupName, message);
    }

    // Information Methods
    public Task<int> GetConnectionCount()
    {
        return Task.FromResult(_connections.Count);
    }

    public Task<object> GetConnectionInfo()
    {
        return Task.FromResult((object)new
        {
            ConnectionId = Context.ConnectionId,
            TotalConnections = _connections.Count,
            ConnectedAt = DateTime.UtcNow,
            UserIdentifier = Context.UserIdentifier
        });
    }

    public Task<object> GetGroupInfo()
    {
        lock (_lock)
        {
            return Task.FromResult((object)_groups.ToDictionary(
                kvp => kvp.Key,
                kvp => new
                {
                    GroupName = kvp.Key,
                    MemberCount = kvp.Value.Count,
                    Members = kvp.Value.ToArray()
                }
            ));
        }
    }

    // Performance Testing Methods
    public async Task SendBulkMessages(int count)
    {
        var tasks = new List<Task>();
        for (int i = 0; i < count; i++)
        {
            tasks.Add(Clients.Caller.SendAsync("BulkMessage", i, $"Bulk message {i}", DateTime.UtcNow.ToString("O")));
        }
        await Task.WhenAll(tasks);
    }

    public async Task SendLargeMessage(int sizeKb)
    {
        var message = new string('A', sizeKb * 1024);
        await Clients.Caller.SendAsync("LargeMessage", sizeKb, message, DateTime.UtcNow.ToString("O"));
    }

    public async Task StressTest(int messageCount, int delayMs)
    {
        for (int i = 0; i < messageCount; i++)
        {
            await Clients.Caller.SendAsync("StressTestMessage", i, $"Stress test message {i}", DateTime.UtcNow.ToString("O"));
            if (delayMs > 0)
            {
                await Task.Delay(delayMs);
            }
        }
    }
}