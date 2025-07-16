using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace SignalRPerformanceTests
{
    class Program
    {
        private static readonly ConcurrentBag<HubConnection> _connections = new();
        private static readonly ConcurrentBag<TimeSpan> _responseTimes = new();
        private static int _totalMessages = 0;
        private static int _totalErrors = 0;
        private static DateTime _testStartTime;
        private static bool _testRunning = false;

        static async Task Main(string[] args)
        {
            Console.WriteLine("SignalR Performance Testing Tool");
            Console.WriteLine("=================================");
            
            var serverUrl = args.Length > 0 ? args[0] : "https://localhost:7039/chathub";
            Console.WriteLine($"Target Server: {serverUrl}");
            
            await ShowMainMenu(serverUrl);
        }

        private static async Task ShowMainMenu(string serverUrl)
        {
            while (true)
            {
                Console.WriteLine("\n=== Performance Test Menu ===");
                Console.WriteLine("1. Connection Density Test");
                Console.WriteLine("2. Message Throughput Test");
                Console.WriteLine("3. Echo Response Time Test");
                Console.WriteLine("4. Concurrent User Simulation");
                Console.WriteLine("5. Stress Test");
                Console.WriteLine("6. Show Statistics");
                Console.WriteLine("7. Clear Statistics");
                Console.WriteLine("8. Cleanup Connections");
                Console.WriteLine("0. Exit");
                Console.Write("Select an option: ");
                
                var choice = Console.ReadLine();
                
                switch (choice)
                {
                    case "1":
                        await ConnectionDensityTest(serverUrl);
                        break;
                    case "2":
                        await MessageThroughputTest(serverUrl);
                        break;
                    case "3":
                        await EchoResponseTimeTest(serverUrl);
                        break;
                    case "4":
                        await ConcurrentUserSimulation(serverUrl);
                        break;
                    case "5":
                        await StressTest(serverUrl);
                        break;
                    case "6":
                        ShowStatistics();
                        break;
                    case "7":
                        ClearStatistics();
                        break;
                    case "8":
                        await CleanupConnections();
                        break;
                    case "0":
                        await CleanupConnections();
                        Console.WriteLine("Goodbye!");
                        return;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
        }

        private static async Task ConnectionDensityTest(string serverUrl)
        {
            Console.Write("Enter number of connections to test (default 100): ");
            var input = Console.ReadLine();
            var connectionCount = int.TryParse(input, out var parsed) ? parsed : 100;
            
            Console.Write("Enter connection interval in ms (default 50): ");
            var intervalInput = Console.ReadLine();
            var interval = int.TryParse(intervalInput, out var intervalParsed) ? intervalParsed : 50;
            
            Console.WriteLine($"Testing connection density with {connectionCount} connections...");
            
            var stopwatch = Stopwatch.StartNew();
            var successfulConnections = 0;
            var failedConnections = 0;
            
            for (int i = 0; i < connectionCount; i++)
            {
                try
                {
                    var connection = new HubConnectionBuilder()
                        .WithUrl(serverUrl)
                        .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Warning))
                        .Build();
                    
                    await connection.StartAsync();
                    _connections.Add(connection);
                    successfulConnections++;
                    
                    if (i % 10 == 0)
                    {
                        Console.WriteLine($"Connected {i + 1}/{connectionCount} clients...");
                    }
                    
                    if (interval > 0)
                    {
                        await Task.Delay(interval);
                    }
                }
                catch (Exception ex)
                {
                    failedConnections++;
                    Console.WriteLine($"Connection {i + 1} failed: {ex.Message}");
                }
            }
            
            stopwatch.Stop();
            
            Console.WriteLine($"\n=== Connection Density Test Results ===");
            Console.WriteLine($"Successful Connections: {successfulConnections}");
            Console.WriteLine($"Failed Connections: {failedConnections}");
            Console.WriteLine($"Total Time: {stopwatch.Elapsed}");
            Console.WriteLine($"Connections per second: {successfulConnections / stopwatch.Elapsed.TotalSeconds:F2}");
        }

        private static async Task MessageThroughputTest(string serverUrl)
        {
            Console.Write("Enter number of messages per connection (default 100): ");
            var messagesInput = Console.ReadLine();
            var messagesPerConnection = int.TryParse(messagesInput, out var messagesParsed) ? messagesParsed : 100;
            
            Console.Write("Enter number of concurrent connections (default 10): ");
            var connectionsInput = Console.ReadLine();
            var connectionCount = int.TryParse(connectionsInput, out var connectionsParsed) ? connectionsParsed : 10;
            
            Console.WriteLine($"Testing message throughput with {connectionCount} connections, {messagesPerConnection} messages each...");
            
            // Create connections
            var connections = new List<HubConnection>();
            for (int i = 0; i < connectionCount; i++)
            {
                var connection = new HubConnectionBuilder()
                    .WithUrl(serverUrl)
                    .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Warning))
                    .Build();
                
                await connection.StartAsync();
                connections.Add(connection);
            }
            
            _testStartTime = DateTime.UtcNow;
            _testRunning = true;
            var stopwatch = Stopwatch.StartNew();
            
            // Send messages concurrently
            var tasks = connections.Select(async connection =>
            {
                for (int i = 0; i < messagesPerConnection; i++)
                {
                    try
                    {
                        await connection.InvokeAsync("SendMessage", $"Test message {i}");
                        Interlocked.Increment(ref _totalMessages);
                    }
                    catch (Exception)
                    {
                        Interlocked.Increment(ref _totalErrors);
                    }
                }
            });
            
            await Task.WhenAll(tasks);
            stopwatch.Stop();
            _testRunning = false;
            
            Console.WriteLine($"\n=== Message Throughput Test Results ===");
            Console.WriteLine($"Total Messages Sent: {_totalMessages}");
            Console.WriteLine($"Total Errors: {_totalErrors}");
            Console.WriteLine($"Total Time: {stopwatch.Elapsed}");
            Console.WriteLine($"Messages per second: {_totalMessages / stopwatch.Elapsed.TotalSeconds:F2}");
            
            // Cleanup test connections
            foreach (var connection in connections)
            {
                try
                {
                    await connection.DisposeAsync();
                }
                catch { }
            }
        }

        private static async Task EchoResponseTimeTest(string serverUrl)
        {
            Console.Write("Enter number of echo tests (default 100): ");
            var input = Console.ReadLine();
            var echoCount = int.TryParse(input, out var parsed) ? parsed : 100;
            
            Console.Write("Enter concurrent connections (default 5): ");
            var connectionsInput = Console.ReadLine();
            var connectionCount = int.TryParse(connectionsInput, out var connectionsParsed) ? connectionsParsed : 5;
            
            Console.WriteLine($"Testing echo response time with {connectionCount} connections, {echoCount} echoes each...");
            
            var connections = new List<HubConnection>();
            var responseTimes = new ConcurrentBag<TimeSpan>();
            
            // Create connections with echo handlers
            for (int i = 0; i < connectionCount; i++)
            {
                var connection = new HubConnectionBuilder()
                    .WithUrl(serverUrl)
                    .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Warning))
                    .Build();
                
                connection.On<string, DateTime>("EchoResponse", (message, timestamp) =>
                {
                    var responseTime = DateTime.UtcNow - timestamp;
                    responseTimes.Add(responseTime);
                });
                
                await connection.StartAsync();
                connections.Add(connection);
            }
            
            var stopwatch = Stopwatch.StartNew();
            
            // Send echo messages concurrently
            var tasks = connections.Select(async connection =>
            {
                for (int i = 0; i < echoCount; i++)
                {
                    try
                    {
                        await connection.InvokeAsync("Echo", $"Echo test {i}");
                    }
                    catch (Exception)
                    {
                        Interlocked.Increment(ref _totalErrors);
                    }
                }
            });
            
            await Task.WhenAll(tasks);
            
            // Wait for all responses
            var waitTime = 0;
            while (responseTimes.Count < (echoCount * connectionCount) && waitTime < 30000)
            {
                await Task.Delay(100);
                waitTime += 100;
            }
            
            stopwatch.Stop();
            
            Console.WriteLine($"\n=== Echo Response Time Test Results ===");
            Console.WriteLine($"Total Echoes Sent: {echoCount * connectionCount}");
            Console.WriteLine($"Total Responses Received: {responseTimes.Count}");
            Console.WriteLine($"Total Errors: {_totalErrors}");
            Console.WriteLine($"Total Time: {stopwatch.Elapsed}");
            
            if (responseTimes.Any())
            {
                var times = responseTimes.Select(rt => rt.TotalMilliseconds).OrderBy(t => t).ToArray();
                Console.WriteLine($"Average Response Time: {times.Average():F2}ms");
                Console.WriteLine($"Median Response Time: {times[times.Length / 2]:F2}ms");
                Console.WriteLine($"Min Response Time: {times.Min():F2}ms");
                Console.WriteLine($"Max Response Time: {times.Max():F2}ms");
                Console.WriteLine($"95th Percentile: {times[(int)(times.Length * 0.95)]:F2}ms");
            }
            
            // Cleanup test connections
            foreach (var connection in connections)
            {
                try
                {
                    await connection.DisposeAsync();
                }
                catch { }
            }
        }

        private static async Task ConcurrentUserSimulation(string serverUrl)
        {
            Console.Write("Enter number of simulated users (default 50): ");
            var usersInput = Console.ReadLine();
            var userCount = int.TryParse(usersInput, out var usersParsed) ? usersParsed : 50;
            
            Console.Write("Enter test duration in seconds (default 60): ");
            var durationInput = Console.ReadLine();
            var duration = int.TryParse(durationInput, out var durationParsed) ? durationParsed : 60;
            
            Console.WriteLine($"Simulating {userCount} concurrent users for {duration} seconds...");
            
            var connections = new List<HubConnection>();
            var random = new Random();
            _testStartTime = DateTime.UtcNow;
            _testRunning = true;
            
            // Create user connections
            for (int i = 0; i < userCount; i++)
            {
                var connection = new HubConnectionBuilder()
                    .WithUrl(serverUrl)
                    .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Warning))
                    .Build();
                
                await connection.StartAsync();
                connections.Add(connection);
                
                // Join random groups
                if (random.Next(0, 100) < 30) // 30% chance to join a group
                {
                    var groupName = $"Group{random.Next(1, 6)}";
                    await connection.InvokeAsync("JoinGroup", groupName);
                }
            }
            
            var stopwatch = Stopwatch.StartNew();
            
            // Simulate user activity
            var tasks = connections.Select(async connection =>
            {
                while (stopwatch.Elapsed.TotalSeconds < duration)
                {
                    try
                    {
                        var action = random.Next(0, 100);
                        if (action < 60) // 60% send message
                        {
                            await connection.InvokeAsync("SendMessage", $"User message {DateTime.UtcNow.Ticks}");
                        }
                        else if (action < 80) // 20% echo test
                        {
                            await connection.InvokeAsync("Echo", $"Echo {DateTime.UtcNow.Ticks}");
                        }
                        else // 20% broadcast
                        {
                            await connection.InvokeAsync("Broadcast", $"Broadcast {DateTime.UtcNow.Ticks}");
                        }
                        
                        Interlocked.Increment(ref _totalMessages);
                        
                        // Random delay between actions
                        await Task.Delay(random.Next(500, 3000));
                    }
                    catch (Exception)
                    {
                        Interlocked.Increment(ref _totalErrors);
                    }
                }
            });
            
            await Task.WhenAll(tasks);
            stopwatch.Stop();
            _testRunning = false;
            
            Console.WriteLine($"\n=== Concurrent User Simulation Results ===");
            Console.WriteLine($"Users: {userCount}");
            Console.WriteLine($"Duration: {stopwatch.Elapsed}");
            Console.WriteLine($"Total Messages Sent: {_totalMessages}");
            Console.WriteLine($"Total Errors: {_totalErrors}");
            Console.WriteLine($"Messages per second: {_totalMessages / stopwatch.Elapsed.TotalSeconds:F2}");
            Console.WriteLine($"Average messages per user: {_totalMessages / (double)userCount:F2}");
            
            // Cleanup test connections
            foreach (var connection in connections)
            {
                try
                {
                    await connection.DisposeAsync();
                }
                catch { }
            }
        }

        private static async Task StressTest(string serverUrl)
        {
            Console.Write("Enter number of connections (default 20): ");
            var connectionsInput = Console.ReadLine();
            var connectionCount = int.TryParse(connectionsInput, out var connectionsParsed) ? connectionsParsed : 20;
            
            Console.Write("Enter messages per connection (default 200): ");
            var messagesInput = Console.ReadLine();
            var messagesPerConnection = int.TryParse(messagesInput, out var messagesParsed) ? messagesParsed : 200;
            
            Console.Write("Enter delay between messages in ms (default 10): ");
            var delayInput = Console.ReadLine();
            var delay = int.TryParse(delayInput, out var delayParsed) ? delayParsed : 10;
            
            Console.WriteLine($"Starting stress test with {connectionCount} connections, {messagesPerConnection} messages each, {delay}ms delay...");
            
            var connections = new List<HubConnection>();
            
            // Create connections
            for (int i = 0; i < connectionCount; i++)
            {
                var connection = new HubConnectionBuilder()
                    .WithUrl(serverUrl)
                    .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Warning))
                    .Build();
                
                await connection.StartAsync();
                connections.Add(connection);
            }
            
            _testStartTime = DateTime.UtcNow;
            _testRunning = true;
            var stopwatch = Stopwatch.StartNew();
            
            // Run stress test
            var tasks = connections.Select(async connection =>
            {
                await connection.InvokeAsync("StressTest", messagesPerConnection, delay);
            });
            
            await Task.WhenAll(tasks);
            stopwatch.Stop();
            _testRunning = false;
            
            Console.WriteLine($"\n=== Stress Test Results ===");
            Console.WriteLine($"Connections: {connectionCount}");
            Console.WriteLine($"Messages per connection: {messagesPerConnection}");
            Console.WriteLine($"Total expected messages: {connectionCount * messagesPerConnection}");
            Console.WriteLine($"Total time: {stopwatch.Elapsed}");
            
            // Cleanup test connections
            foreach (var connection in connections)
            {
                try
                {
                    await connection.DisposeAsync();
                }
                catch { }
            }
        }

        private static void ShowStatistics()
        {
            Console.WriteLine("\n=== Performance Statistics ===");
            Console.WriteLine($"Total Messages Sent: {_totalMessages}");
            Console.WriteLine($"Total Errors: {_totalErrors}");
            Console.WriteLine($"Active Connections: {_connections.Count}");
            Console.WriteLine($"Test Running: {_testRunning}");
            
            if (_testStartTime != default && _testRunning)
            {
                var duration = DateTime.UtcNow - _testStartTime;
                Console.WriteLine($"Current Test Duration: {duration}");
                Console.WriteLine($"Messages per second: {_totalMessages / duration.TotalSeconds:F2}");
            }
            
            if (_responseTimes.Any())
            {
                var times = _responseTimes.Select(rt => rt.TotalMilliseconds).ToArray();
                Console.WriteLine($"Response Times - Avg: {times.Average():F2}ms, Min: {times.Min():F2}ms, Max: {times.Max():F2}ms");
            }
        }

        private static void ClearStatistics()
        {
            _totalMessages = 0;
            _totalErrors = 0;
            _responseTimes.Clear();
            _testStartTime = default;
            _testRunning = false;
            Console.WriteLine("✅ Statistics cleared!");
        }

        private static async Task CleanupConnections()
        {
            Console.WriteLine("Cleaning up connections...");
            var cleanupTasks = new List<Task>();
            
            while (_connections.TryTake(out var connection))
            {
                cleanupTasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        await connection.DisposeAsync();
                    }
                    catch { }
                }));
            }
            
            await Task.WhenAll(cleanupTasks);
            Console.WriteLine($"✅ Cleaned up {cleanupTasks.Count} connections");
        }
    }
}