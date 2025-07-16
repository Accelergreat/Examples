using Microsoft.AspNetCore.SignalR.Client;
using System.Diagnostics;

namespace SignalRClient;

class Program
{
    private static HubConnection? _connection;
    private static readonly List<double> _responseTimes = new();
    private static readonly object _lock = new();
    private static int _messagesReceived = 0;
    private static readonly Stopwatch _overallTimer = new();

    static async Task Main(string[] args)
    {
        string url = args.Length > 0 ? args[0] : "https://localhost:7039/chathub";
        
        Console.WriteLine("SignalR Test Client");
        Console.WriteLine("==================");
        Console.WriteLine($"Connecting to: {url}");
        Console.WriteLine();

        await ConnectAsync(url);
        
        if (_connection != null)
        {
            await ShowMenuAsync();
        }
    }

    private static async Task ConnectAsync(string url)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(url)
            .WithAutomaticReconnect()
            .Build();

        // Set up event handlers
        SetupEventHandlers();

        try
        {
            await _connection.StartAsync();
            Console.WriteLine("✅ Connected to SignalR hub!");
            Console.WriteLine($"Connection ID: {_connection.ConnectionId}");
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Connection failed: {ex.Message}");
            Console.WriteLine("Make sure the SignalR server is running on the specified URL.");
            Console.WriteLine();
        }
    }

    private static void SetupEventHandlers()
    {
        if (_connection == null) return;

        // Connection events
        _connection.On<string, int>("UserConnected", (connectionId, count) =>
        {
            Console.WriteLine($"🔗 User connected: {connectionId} (Total: {count})");
        });

        _connection.On<string, int>("UserDisconnected", (connectionId, count) =>
        {
            Console.WriteLine($"🔌 User disconnected: {connectionId} (Total: {count})");
        });

        // Message events
        _connection.On<string, string>("ReceiveMessage", (connectionId, message) =>
        {
            Console.WriteLine($"📨 Message from {connectionId}: {message}");
            IncrementMessageCount();
        });

        _connection.On<string, string>("ReceivePrivateMessage", (connectionId, message) =>
        {
            Console.WriteLine($"🔒 Private message from {connectionId}: {message}");
            IncrementMessageCount();
        });

        _connection.On<string, string>("EchoResponse", (message, timestamp) =>
        {
            var responseTime = CalculateResponseTime(timestamp);
            Console.WriteLine($"🔄 Echo: {message} (Response time: {responseTime:F2}ms)");
            lock (_lock)
            {
                _responseTimes.Add(responseTime);
            }
            IncrementMessageCount();
        });

        _connection.On<string, string, DateTime>("BroadcastMessage", (connectionId, message, timestamp) =>
        {
            Console.WriteLine($"📢 Broadcast from {connectionId}: {message} at {timestamp:HH:mm:ss}");
            IncrementMessageCount();
        });

        // Group events
        _connection.On<string, string>("UserJoinedGroup", (connectionId, groupName) =>
        {
            Console.WriteLine($"👥 User {connectionId} joined group '{groupName}'");
        });

        _connection.On<string, string>("UserLeftGroup", (connectionId, groupName) =>
        {
            Console.WriteLine($"👋 User {connectionId} left group '{groupName}'");
        });

        _connection.On<string, string, string>("ReceiveGroupMessage", (connectionId, groupName, message) =>
        {
            Console.WriteLine($"👥 Group '{groupName}' message from {connectionId}: {message}");
            IncrementMessageCount();
        });

        // Performance testing events
        _connection.On<int, string, string>("BulkMessage", (index, message, timestamp) =>
        {
            IncrementMessageCount();
        });

        _connection.On<int, string, string>("LargeMessage", (sizeKb, message, timestamp) =>
        {
            var responseTime = CalculateResponseTime(timestamp);
            Console.WriteLine($"📦 Large message ({sizeKb}KB) received (Response time: {responseTime:F2}ms)");
            IncrementMessageCount();
        });

        _connection.On<int, string, string>("StressTestMessage", (index, message, timestamp) =>
        {
            IncrementMessageCount();
        });

        // Reconnection events
        _connection.Reconnecting += (error) =>
        {
            Console.WriteLine("🔄 Reconnecting...");
            return Task.CompletedTask;
        };

        _connection.Reconnected += (connectionId) =>
        {
            Console.WriteLine($"✅ Reconnected with ID: {connectionId}");
            return Task.CompletedTask;
        };

        _connection.Closed += (error) =>
        {
            Console.WriteLine($"❌ Connection closed: {error?.Message}");
            return Task.CompletedTask;
        };
    }

    private static async Task ShowMenuAsync()
    {
        if (_connection == null) return;

        while (true)
        {
            Console.WriteLine("\n=== SignalR Test Menu ===");
            Console.WriteLine("1. Send Message");
            Console.WriteLine("2. Echo Test");
            Console.WriteLine("3. Broadcast Test");
            Console.WriteLine("4. Group Tests");
            Console.WriteLine("5. Performance Tests");
            Console.WriteLine("6. Connection Tests");
            Console.WriteLine("7. Show Statistics");
            Console.WriteLine("8. Clear Statistics");
            Console.WriteLine("0. Exit");
            Console.Write("\nSelect an option: ");

            var input = Console.ReadLine();
            Console.WriteLine();

            switch (input)
            {
                case "1":
                    await SendMessageAsync();
                    break;
                case "2":
                    await EchoTestAsync();
                    break;
                case "3":
                    await BroadcastTestAsync();
                    break;
                case "4":
                    await GroupTestsAsync();
                    break;
                case "5":
                    await PerformanceTestsAsync();
                    break;
                case "6":
                    await ConnectionTestsAsync();
                    break;
                case "7":
                    ShowStatistics();
                    break;
                case "8":
                    ClearStatistics();
                    break;
                case "0":
                    await _connection.StopAsync();
                    return;
                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }
    }

    private static async Task SendMessageAsync()
    {
        Console.Write("Enter message: ");
        var message = Console.ReadLine();
        if (!string.IsNullOrEmpty(message))
        {
            await _connection!.InvokeAsync("SendMessage", message);
            Console.WriteLine("Message sent!");
        }
    }

    private static async Task EchoTestAsync()
    {
        Console.Write("Enter number of echo tests (default 10): ");
        var input = Console.ReadLine();
        int count = int.TryParse(input, out int result) ? result : 10;

        Console.WriteLine($"Starting {count} echo tests...");
        var stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < count; i++)
        {
            await _connection!.InvokeAsync("Echo", $"Echo message {i + 1}");
            await Task.Delay(100); // Small delay between messages
        }

        stopwatch.Stop();
        Console.WriteLine($"Echo tests completed in {stopwatch.ElapsedMilliseconds}ms");
    }

    private static async Task BroadcastTestAsync()
    {
        Console.Write("Enter broadcast message: ");
        var message = Console.ReadLine();
        if (!string.IsNullOrEmpty(message))
        {
            await _connection!.InvokeAsync("Broadcast", message);
            Console.WriteLine("Broadcast message sent!");
        }
    }

    private static async Task GroupTestsAsync()
    {
        while (true)
        {
            Console.WriteLine("\n=== Group Tests ===");
            Console.WriteLine("1. Join Group");
            Console.WriteLine("2. Leave Group");
            Console.WriteLine("3. Send Message to Group");
            Console.WriteLine("4. Get Group Info");
            Console.WriteLine("0. Back to Main Menu");
            Console.Write("Select an option: ");

            var input = Console.ReadLine();
            Console.WriteLine();

            switch (input)
            {
                case "1":
                    Console.Write("Enter group name: ");
                    var groupName = Console.ReadLine();
                    if (!string.IsNullOrEmpty(groupName))
                    {
                        await _connection!.InvokeAsync("JoinGroup", groupName);
                        Console.WriteLine($"Joined group '{groupName}'");
                    }
                    break;
                case "2":
                    Console.Write("Enter group name: ");
                    var leaveGroupName = Console.ReadLine();
                    if (!string.IsNullOrEmpty(leaveGroupName))
                    {
                        await _connection!.InvokeAsync("LeaveGroup", leaveGroupName);
                        Console.WriteLine($"Left group '{leaveGroupName}'");
                    }
                    break;
                case "3":
                    Console.Write("Enter group name: ");
                    var msgGroupName = Console.ReadLine();
                    Console.Write("Enter message: ");
                    var groupMessage = Console.ReadLine();
                    if (!string.IsNullOrEmpty(msgGroupName) && !string.IsNullOrEmpty(groupMessage))
                    {
                        await _connection!.InvokeAsync("SendMessageToGroup", msgGroupName, groupMessage);
                        Console.WriteLine($"Message sent to group '{msgGroupName}'");
                    }
                    break;
                case "4":
                    var groupInfo = await _connection!.InvokeAsync<object>("GetGroupInfo");
                    Console.WriteLine($"Group Info: {groupInfo}");
                    break;
                case "0":
                    return;
                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }
    }

    private static async Task PerformanceTestsAsync()
    {
        while (true)
        {
            Console.WriteLine("\n=== Performance Tests ===");
            Console.WriteLine("1. Bulk Message Test");
            Console.WriteLine("2. Large Message Test");
            Console.WriteLine("3. Stress Test");
            Console.WriteLine("0. Back to Main Menu");
            Console.Write("Select an option: ");

            var input = Console.ReadLine();
            Console.WriteLine();

            switch (input)
            {
                case "1":
                    Console.Write("Enter number of messages (default 100): ");
                    var bulkInput = Console.ReadLine();
                    int bulkCount = int.TryParse(bulkInput, out int bulkResult) ? bulkResult : 100;
                    
                    Console.WriteLine($"Sending {bulkCount} bulk messages...");
                    var bulkTimer = Stopwatch.StartNew();
                    int beforeBulk = _messagesReceived;
                    
                    await _connection!.InvokeAsync("SendBulkMessages", bulkCount);
                    
                    bulkTimer.Stop();
                    Console.WriteLine($"Bulk messages completed in {bulkTimer.ElapsedMilliseconds}ms");
                    Console.WriteLine($"Messages received: {_messagesReceived - beforeBulk}");
                    break;
                case "2":
                    Console.Write("Enter message size in KB (default 10): ");
                    var sizeInput = Console.ReadLine();
                    int sizeKb = int.TryParse(sizeInput, out int sizeResult) ? sizeResult : 10;
                    
                    Console.WriteLine($"Sending {sizeKb}KB message...");
                    var timestamp = DateTime.UtcNow.ToString("O");
                    await _connection!.InvokeAsync("SendLargeMessage", sizeKb);
                    break;
                case "3":
                    Console.Write("Enter number of messages (default 50): ");
                    var stressCountInput = Console.ReadLine();
                    int stressCount = int.TryParse(stressCountInput, out int stressResult) ? stressResult : 50;
                    
                    Console.Write("Enter delay between messages in ms (default 100): ");
                    var delayInput = Console.ReadLine();
                    int delay = int.TryParse(delayInput, out int delayResult) ? delayResult : 100;
                    
                    Console.WriteLine($"Starting stress test: {stressCount} messages with {delay}ms delay...");
                    var stressTimer = Stopwatch.StartNew();
                    int beforeStress = _messagesReceived;
                    
                    await _connection!.InvokeAsync("StressTest", stressCount, delay);
                    
                    stressTimer.Stop();
                    Console.WriteLine($"Stress test completed in {stressTimer.ElapsedMilliseconds}ms");
                    Console.WriteLine($"Messages received: {_messagesReceived - beforeStress}");
                    break;
                case "0":
                    return;
                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }
    }

    private static async Task ConnectionTestsAsync()
    {
        while (true)
        {
            Console.WriteLine("\n=== Connection Tests ===");
            Console.WriteLine("1. Get Connection Count");
            Console.WriteLine("2. Get Connection Info");
            Console.WriteLine("3. Test Connection State");
            Console.WriteLine("0. Back to Main Menu");
            Console.Write("Select an option: ");

            var input = Console.ReadLine();
            Console.WriteLine();

            switch (input)
            {
                case "1":
                    var count = await _connection!.InvokeAsync<int>("GetConnectionCount");
                    Console.WriteLine($"Active connections: {count}");
                    break;
                case "2":
                    var info = await _connection!.InvokeAsync<object>("GetConnectionInfo");
                    Console.WriteLine($"Connection Info: {info}");
                    break;
                case "3":
                    Console.WriteLine($"Connection State: {_connection!.State}");
                    Console.WriteLine($"Connection ID: {_connection.ConnectionId}");
                    break;
                case "0":
                    return;
                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }
    }

    private static void ShowStatistics()
    {
        Console.WriteLine("\n=== Statistics ===");
        Console.WriteLine($"Total messages received: {_messagesReceived}");
        
        lock (_lock)
        {
            if (_responseTimes.Count > 0)
            {
                Console.WriteLine($"Echo tests performed: {_responseTimes.Count}");
                Console.WriteLine($"Average response time: {_responseTimes.Average():F2}ms");
                Console.WriteLine($"Min response time: {_responseTimes.Min():F2}ms");
                Console.WriteLine($"Max response time: {_responseTimes.Max():F2}ms");
            }
            else
            {
                Console.WriteLine("No echo tests performed yet.");
            }
        }
        
        if (_overallTimer.IsRunning)
        {
            Console.WriteLine($"Session duration: {_overallTimer.Elapsed:hh\\:mm\\:ss}");
            if (_messagesReceived > 0)
            {
                var messagesPerSecond = _messagesReceived / _overallTimer.Elapsed.TotalSeconds;
                Console.WriteLine($"Messages per second: {messagesPerSecond:F2}");
            }
        }
    }

    private static void ClearStatistics()
    {
        lock (_lock)
        {
            _responseTimes.Clear();
            _messagesReceived = 0;
            _overallTimer.Restart();
        }
        Console.WriteLine("Statistics cleared.");
    }

    private static double CalculateResponseTime(string timestamp)
    {
        if (DateTime.TryParse(timestamp, out DateTime sentTime))
        {
            return (DateTime.UtcNow - sentTime).TotalMilliseconds;
        }
        return 0;
    }

    private static void IncrementMessageCount()
    {
        if (!_overallTimer.IsRunning)
        {
            _overallTimer.Start();
        }
        
        Interlocked.Increment(ref _messagesReceived);
    }
}
