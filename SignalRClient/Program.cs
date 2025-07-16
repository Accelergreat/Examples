using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace SignalRClient
{
    class Program
    {
        private static HubConnection? _connection;
        private static int _messageCount = 0;
        private static readonly object _lockObject = new();
        private static readonly List<TimeSpan> _responseTimes = new();
        private static DateTime _testStartTime;

        static async Task Main(string[] args)
        {
            Console.WriteLine("SignalR Test Client");
            Console.WriteLine("===================");
            
            // Default server URL
            var serverUrl = args.Length > 0 ? args[0] : "https://localhost:7039/chathub";
            
            Console.WriteLine($"Connecting to: {serverUrl}");
            
            // Create connection
            _connection = new HubConnectionBuilder()
                .WithUrl(serverUrl)
                .ConfigureLogging(logging =>
                {
                    logging.SetMinimumLevel(LogLevel.Information);
                    logging.AddConsole();
                })
                .Build();

            // Set up event handlers
            SetupEventHandlers();

            try
            {
                // Start connection
                await _connection.StartAsync();
                Console.WriteLine("✅ Connected to SignalR hub successfully!");
                
                // Show menu
                await ShowMenu();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error connecting to SignalR hub: {ex.Message}");
                return;
            }
            finally
            {
                if (_connection != null)
                {
                    await _connection.DisposeAsync();
                }
            }
        }

        private static void SetupEventHandlers()
        {
            if (_connection == null) return;

            // Basic message handlers
            _connection.On<string, string, DateTime>("ReceiveMessage", (connectionId, message, timestamp) =>
            {
                Console.WriteLine($"[{timestamp:HH:mm:ss}] {connectionId}: {message}");
                lock (_lockObject)
                {
                    _messageCount++;
                    _responseTimes.Add(DateTime.UtcNow - timestamp);
                }
            });

            _connection.On<string, string, DateTime>("ReceivePrivateMessage", (connectionId, message, timestamp) =>
            {
                Console.WriteLine($"[Private] [{timestamp:HH:mm:ss}] {connectionId}: {message}");
            });

            _connection.On<string, string, string, DateTime>("ReceiveGroupMessage", (connectionId, groupName, message, timestamp) =>
            {
                Console.WriteLine($"[Group: {groupName}] [{timestamp:HH:mm:ss}] {connectionId}: {message}");
            });

            _connection.On<string, DateTime>("EchoResponse", (message, timestamp) =>
            {
                var responseTime = DateTime.UtcNow - timestamp;
                Console.WriteLine($"[Echo] {message} (Response time: {responseTime.TotalMilliseconds:F2}ms)");
                lock (_lockObject)
                {
                    _responseTimes.Add(responseTime);
                }
            });

            _connection.On<string, string, DateTime>("BroadcastMessage", (connectionId, message, timestamp) =>
            {
                Console.WriteLine($"[Broadcast] [{timestamp:HH:mm:ss}] {connectionId}: {message}");
            });

            // Connection management handlers
            _connection.On<string, int>("UserConnected", (connectionId, totalConnections) =>
            {
                Console.WriteLine($"[Info] User connected: {connectionId} (Total: {totalConnections})");
            });

            _connection.On<string, int>("UserDisconnected", (connectionId, totalConnections) =>
            {
                Console.WriteLine($"[Info] User disconnected: {connectionId} (Total: {totalConnections})");
            });

            _connection.On<string, string>("UserJoinedGroup", (connectionId, groupName) =>
            {
                Console.WriteLine($"[Info] User {connectionId} joined group: {groupName}");
            });

            _connection.On<string, string>("UserLeftGroup", (connectionId, groupName) =>
            {
                Console.WriteLine($"[Info] User {connectionId} left group: {groupName}");
            });

            _connection.On<int>("ConnectionCount", (count) =>
            {
                Console.WriteLine($"[Info] Total connections: {count}");
            });

            // Performance testing handlers
            _connection.On<int, string, DateTime>("BulkMessage", (index, message, timestamp) =>
            {
                if (index % 100 == 0)
                {
                    Console.WriteLine($"[Bulk] Received message {index}");
                }
                lock (_lockObject)
                {
                    _messageCount++;
                }
            });

            _connection.On<string, int, DateTime>("LargeMessage", (message, sizeKb, timestamp) =>
            {
                var responseTime = DateTime.UtcNow - timestamp;
                Console.WriteLine($"[Large] Received {sizeKb}KB message (Response time: {responseTime.TotalMilliseconds:F2}ms)");
            });

            _connection.On<int, string, DateTime>("StressTestMessage", (index, message, timestamp) =>
            {
                if (index % 50 == 0)
                {
                    Console.WriteLine($"[Stress] Received message {index}");
                }
                lock (_lockObject)
                {
                    _messageCount++;
                }
            });

            // Connection state change handlers
            _connection.Closed += async (error) =>
            {
                Console.WriteLine($"[Warning] Connection closed: {error?.Message}");
                await Task.Delay(new Random().Next(0, 5) * 1000);
                // Attempt to reconnect
                await _connection.StartAsync();
            };

            _connection.Reconnected += (connectionId) =>
            {
                Console.WriteLine($"[Info] Reconnected with connection ID: {connectionId}");
                return Task.CompletedTask;
            };

            _connection.Reconnecting += (error) =>
            {
                Console.WriteLine($"[Warning] Attempting to reconnect: {error?.Message}");
                return Task.CompletedTask;
            };
        }

        private static async Task ShowMenu()
        {
            while (true)
            {
                Console.WriteLine("\n=== SignalR Test Menu ===");
                Console.WriteLine("1. Send Message");
                Console.WriteLine("2. Echo Test");
                Console.WriteLine("3. Broadcast Test");
                Console.WriteLine("4. Group Tests");
                Console.WriteLine("5. Performance Tests");
                Console.WriteLine("6. Connection Tests");
                Console.WriteLine("7. Statistics");
                Console.WriteLine("8. Clear Statistics");
                Console.WriteLine("0. Exit");
                Console.Write("Select an option: ");
                
                var choice = Console.ReadLine();
                
                switch (choice)
                {
                    case "1":
                        await SendMessageTest();
                        break;
                    case "2":
                        await EchoTest();
                        break;
                    case "3":
                        await BroadcastTest();
                        break;
                    case "4":
                        await GroupTests();
                        break;
                    case "5":
                        await PerformanceTests();
                        break;
                    case "6":
                        await ConnectionTests();
                        break;
                    case "7":
                        ShowStatistics();
                        break;
                    case "8":
                        ClearStatistics();
                        break;
                    case "0":
                        Console.WriteLine("Goodbye!");
                        return;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
        }

        private static async Task SendMessageTest()
        {
            Console.Write("Enter message to send: ");
            var message = Console.ReadLine();
            
            if (!string.IsNullOrWhiteSpace(message))
            {
                try
                {
                    await _connection!.InvokeAsync("SendMessage", message);
                    Console.WriteLine("✅ Message sent successfully!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error sending message: {ex.Message}");
                }
            }
        }

        private static async Task EchoTest()
        {
            Console.Write("Enter number of echo tests (default 10): ");
            var input = Console.ReadLine();
            var count = int.TryParse(input, out var parsed) ? parsed : 10;
            
            Console.WriteLine($"Starting echo test with {count} messages...");
            _testStartTime = DateTime.UtcNow;
            
            var tasks = new List<Task>();
            for (int i = 0; i < count; i++)
            {
                tasks.Add(_connection!.InvokeAsync("Echo", $"Echo message {i + 1}"));
            }
            
            await Task.WhenAll(tasks);
            Console.WriteLine($"✅ Echo test completed! Sent {count} messages.");
        }

        private static async Task BroadcastTest()
        {
            Console.Write("Enter message to broadcast: ");
            var message = Console.ReadLine();
            
            if (!string.IsNullOrWhiteSpace(message))
            {
                try
                {
                    await _connection!.InvokeAsync("Broadcast", message);
                    Console.WriteLine("✅ Broadcast sent successfully!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error sending broadcast: {ex.Message}");
                }
            }
        }

        private static async Task GroupTests()
        {
            Console.WriteLine("\n=== Group Tests ===");
            Console.WriteLine("1. Join Group");
            Console.WriteLine("2. Leave Group");
            Console.WriteLine("3. Send Message to Group");
            Console.WriteLine("4. Get Group Info");
            Console.Write("Select group test: ");
            
            var choice = Console.ReadLine();
            
            switch (choice)
            {
                case "1":
                    Console.Write("Enter group name to join: ");
                    var joinGroup = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(joinGroup))
                    {
                        await _connection!.InvokeAsync("JoinGroup", joinGroup);
                        Console.WriteLine($"✅ Joined group: {joinGroup}");
                    }
                    break;
                    
                case "2":
                    Console.Write("Enter group name to leave: ");
                    var leaveGroup = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(leaveGroup))
                    {
                        await _connection!.InvokeAsync("LeaveGroup", leaveGroup);
                        Console.WriteLine($"✅ Left group: {leaveGroup}");
                    }
                    break;
                    
                case "3":
                    Console.Write("Enter group name: ");
                    var groupName = Console.ReadLine();
                    Console.Write("Enter message: ");
                    var groupMessage = Console.ReadLine();
                    
                    if (!string.IsNullOrWhiteSpace(groupName) && !string.IsNullOrWhiteSpace(groupMessage))
                    {
                        await _connection!.InvokeAsync("SendMessageToGroup", groupName, groupMessage);
                        Console.WriteLine($"✅ Message sent to group: {groupName}");
                    }
                    break;
                    
                case "4":
                    await _connection!.InvokeAsync("GetGroupInfo");
                    break;
            }
        }

        private static async Task PerformanceTests()
        {
            Console.WriteLine("\n=== Performance Tests ===");
            Console.WriteLine("1. Bulk Message Test");
            Console.WriteLine("2. Large Message Test");
            Console.WriteLine("3. Stress Test");
            Console.Write("Select performance test: ");
            
            var choice = Console.ReadLine();
            
            switch (choice)
            {
                case "1":
                    Console.Write("Enter number of bulk messages (default 1000): ");
                    var bulkInput = Console.ReadLine();
                    var bulkCount = int.TryParse(bulkInput, out var bulkParsed) ? bulkParsed : 1000;
                    
                    Console.WriteLine($"Starting bulk message test with {bulkCount} messages...");
                    _testStartTime = DateTime.UtcNow;
                    _messageCount = 0;
                    
                    await _connection!.InvokeAsync("SendBulkMessages", bulkCount);
                    Console.WriteLine($"✅ Bulk message test initiated!");
                    break;
                    
                case "2":
                    Console.Write("Enter message size in KB (default 100): ");
                    var sizeInput = Console.ReadLine();
                    var sizeKb = int.TryParse(sizeInput, out var sizeParsed) ? sizeParsed : 100;
                    
                    Console.WriteLine($"Starting large message test with {sizeKb}KB message...");
                    var stopwatch = Stopwatch.StartNew();
                    
                    await _connection!.InvokeAsync("SendLargeMessage", sizeKb);
                    
                    stopwatch.Stop();
                    Console.WriteLine($"✅ Large message test completed in {stopwatch.ElapsedMilliseconds}ms");
                    break;
                    
                case "3":
                    Console.Write("Enter number of stress test messages (default 500): ");
                    var stressInput = Console.ReadLine();
                    var stressCount = int.TryParse(stressInput, out var stressParsed) ? stressParsed : 500;
                    
                    Console.Write("Enter delay between messages in ms (default 10): ");
                    var delayInput = Console.ReadLine();
                    var delay = int.TryParse(delayInput, out var delayParsed) ? delayParsed : 10;
                    
                    Console.WriteLine($"Starting stress test with {stressCount} messages, {delay}ms delay...");
                    _testStartTime = DateTime.UtcNow;
                    _messageCount = 0;
                    
                    await _connection!.InvokeAsync("StressTest", stressCount, delay);
                    Console.WriteLine($"✅ Stress test initiated!");
                    break;
            }
        }

        private static async Task ConnectionTests()
        {
            Console.WriteLine("\n=== Connection Tests ===");
            Console.WriteLine("1. Get Connection Count");
            Console.WriteLine("2. Get Connection Info");
            Console.WriteLine("3. Test Connection State");
            Console.Write("Select connection test: ");
            
            var choice = Console.ReadLine();
            
            switch (choice)
            {
                case "1":
                    await _connection!.InvokeAsync("GetConnectionCount");
                    break;
                    
                case "2":
                    await _connection!.InvokeAsync("GetConnectionInfo");
                    break;
                    
                case "3":
                    Console.WriteLine($"Connection State: {_connection!.State}");
                    Console.WriteLine($"Connection ID: {_connection.ConnectionId}");
                    break;
            }
        }

        private static void ShowStatistics()
        {
            Console.WriteLine("\n=== Statistics ===");
            Console.WriteLine($"Total Messages Received: {_messageCount}");
            
            if (_responseTimes.Any())
            {
                var avgResponseTime = _responseTimes.Average(rt => rt.TotalMilliseconds);
                var minResponseTime = _responseTimes.Min(rt => rt.TotalMilliseconds);
                var maxResponseTime = _responseTimes.Max(rt => rt.TotalMilliseconds);
                
                Console.WriteLine($"Average Response Time: {avgResponseTime:F2}ms");
                Console.WriteLine($"Min Response Time: {minResponseTime:F2}ms");
                Console.WriteLine($"Max Response Time: {maxResponseTime:F2}ms");
            }
            
            if (_testStartTime != default)
            {
                var duration = DateTime.UtcNow - _testStartTime;
                var messagesPerSecond = _messageCount / duration.TotalSeconds;
                Console.WriteLine($"Messages Per Second: {messagesPerSecond:F2}");
            }
        }

        private static void ClearStatistics()
        {
            lock (_lockObject)
            {
                _messageCount = 0;
                _responseTimes.Clear();
                _testStartTime = default;
            }
            Console.WriteLine("✅ Statistics cleared!");
        }
    }
}