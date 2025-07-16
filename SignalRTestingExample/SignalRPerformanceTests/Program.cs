using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace SignalRPerformanceTests;

class Program
{
    private static readonly ConcurrentBag<HubConnection> _connections = new();
    private static readonly ConcurrentBag<double> _responseTimes = new();
    private static readonly ConcurrentBag<TimeSpan> _connectionTimes = new();
    private static int _messagesReceived = 0;
    private static int _messagesExpected = 0;
    private static readonly object _lock = new();

    static async Task Main(string[] args)
    {
        Console.WriteLine("SignalR Performance Testing Tool");
        Console.WriteLine("===============================");
        Console.WriteLine();

        string serverUrl = args.Length > 0 ? args[0] : "https://localhost:7039/chathub";
        Console.WriteLine($"Server URL: {serverUrl}");
        Console.WriteLine();

        while (true)
        {
            Console.WriteLine("=== Performance Test Menu ===");
            Console.WriteLine("1. Connection Density Test");
            Console.WriteLine("2. Message Throughput Test");
            Console.WriteLine("3. Echo Response Time Test");
            Console.WriteLine("4. Concurrent Users Test");
            Console.WriteLine("5. Stress Test");
            Console.WriteLine("6. Show Statistics");
            Console.WriteLine("7. Cleanup Connections");
            Console.WriteLine("0. Exit");
            Console.Write("\nSelect an option: ");

            var input = Console.ReadLine();
            Console.WriteLine();

            try
            {
                switch (input)
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
                        await ConcurrentUsersTest(serverUrl);
                        break;
                    case "5":
                        await StressTest(serverUrl);
                        break;
                    case "6":
                        ShowStatistics();
                        break;
                    case "7":
                        await CleanupConnections();
                        break;
                    case "0":
                        await CleanupConnections();
                        return;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
            Console.WriteLine();
        }
    }

    private static async Task ConnectionDensityTest(string serverUrl)
    {
        Console.Write("Enter number of connections to create (default 100): ");
        var input = Console.ReadLine();
        int connectionCount = int.TryParse(input, out int result) ? result : 100;

        Console.Write("Enter connection interval in ms (default 50): ");
        var intervalInput = Console.ReadLine();
        int interval = int.TryParse(intervalInput, out int intervalResult) ? intervalResult : 50;

        Console.WriteLine($"\nStarting connection density test with {connectionCount} connections...");
        Console.WriteLine("Press 'q' to stop early");

        var stopwatch = Stopwatch.StartNew();
        var successfulConnections = 0;
        var failedConnections = 0;

        var cancellationTokenSource = new CancellationTokenSource();
        
        // Start a task to monitor for 'q' key press
        var monitorTask = Task.Run(() =>
        {
            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    if (key.KeyChar == 'q' || key.KeyChar == 'Q')
                    {
                        cancellationTokenSource.Cancel();
                        break;
                    }
                }
                Thread.Sleep(100);
            }
        });

        for (int i = 0; i < connectionCount && !cancellationTokenSource.Token.IsCancellationRequested; i++)
        {
            try
            {
                var connectionStart = Stopwatch.StartNew();
                var connection = CreateConnection(serverUrl);
                await connection.StartAsync();
                connectionStart.Stop();

                _connections.Add(connection);
                _connectionTimes.Add(connectionStart.Elapsed);
                successfulConnections++;

                if (i % 10 == 0)
                {
                    Console.WriteLine($"Created {i + 1} connections... (Success: {successfulConnections}, Failed: {failedConnections})");
                }

                if (interval > 0)
                {
                    await Task.Delay(interval, cancellationTokenSource.Token);
                }
            }
            catch (Exception ex)
            {
                failedConnections++;
                Console.WriteLine($"Connection {i + 1} failed: {ex.Message}");
            }
        }

        stopwatch.Stop();
        cancellationTokenSource.Cancel();

        Console.WriteLine($"\nConnection Density Test Results:");
        Console.WriteLine($"Total time: {stopwatch.Elapsed:mm\\:ss\\.fff}");
        Console.WriteLine($"Successful connections: {successfulConnections}");
        Console.WriteLine($"Failed connections: {failedConnections}");
        Console.WriteLine($"Success rate: {(successfulConnections / (double)(successfulConnections + failedConnections)) * 100:F2}%");
        
        if (_connectionTimes.Count > 0)
        {
            Console.WriteLine($"Average connection time: {_connectionTimes.Average(t => t.TotalMilliseconds):F2}ms");
            Console.WriteLine($"Min connection time: {_connectionTimes.Min(t => t.TotalMilliseconds):F2}ms");
            Console.WriteLine($"Max connection time: {_connectionTimes.Max(t => t.TotalMilliseconds):F2}ms");
        }
    }

    private static async Task MessageThroughputTest(string serverUrl)
    {
        Console.Write("Enter number of messages to send (default 1000): ");
        var input = Console.ReadLine();
        int messageCount = int.TryParse(input, out int result) ? result : 1000;

        Console.Write("Enter number of concurrent connections (default 10): ");
        var connInput = Console.ReadLine();
        int connectionCount = int.TryParse(connInput, out int connResult) ? connResult : 10;

        Console.WriteLine($"\nStarting message throughput test...");
        Console.WriteLine($"Messages: {messageCount}, Connections: {connectionCount}");

        var connections = new List<HubConnection>();
        _messagesReceived = 0;
        _messagesExpected = messageCount * connectionCount;

        // Create connections
        for (int i = 0; i < connectionCount; i++)
        {
            var connection = CreateConnection(serverUrl);
            connection.On<string, string>("EchoResponse", (message, timestamp) =>
            {
                Interlocked.Increment(ref _messagesReceived);
            });
            
            await connection.StartAsync();
            connections.Add(connection);
        }

        Console.WriteLine($"Created {connections.Count} connections");

        var stopwatch = Stopwatch.StartNew();

        // Send messages concurrently
        var tasks = connections.Select(async connection =>
        {
            for (int i = 0; i < messageCount; i++)
            {
                await connection.InvokeAsync("Echo", $"Message {i}");
            }
        });

        await Task.WhenAll(tasks);

        // Wait for all responses
        var timeout = TimeSpan.FromSeconds(30);
        var waitStart = DateTime.UtcNow;
        while (_messagesReceived < _messagesExpected && DateTime.UtcNow - waitStart < timeout)
        {
            await Task.Delay(100);
        }

        stopwatch.Stop();

        Console.WriteLine($"\nMessage Throughput Test Results:");
        Console.WriteLine($"Total time: {stopwatch.Elapsed:mm\\:ss\\.fff}");
        Console.WriteLine($"Messages sent: {_messagesExpected}");
        Console.WriteLine($"Messages received: {_messagesReceived}");
        Console.WriteLine($"Success rate: {(_messagesReceived / (double)_messagesExpected) * 100:F2}%");
        Console.WriteLine($"Messages per second: {_messagesReceived / stopwatch.Elapsed.TotalSeconds:F2}");
        Console.WriteLine($"Average response time: {stopwatch.Elapsed.TotalMilliseconds / _messagesReceived:F2}ms");

        // Cleanup
        foreach (var connection in connections)
        {
            await connection.StopAsync();
        }
    }

    private static async Task EchoResponseTimeTest(string serverUrl)
    {
        Console.Write("Enter number of echo tests (default 100): ");
        var input = Console.ReadLine();
        int echoCount = int.TryParse(input, out int result) ? result : 100;

        Console.WriteLine($"\nStarting echo response time test with {echoCount} messages...");

        var connection = CreateConnection(serverUrl);
        _responseTimes.Clear();

        connection.On<string, string>("EchoResponse", (message, timestamp) =>
        {
            if (DateTime.TryParse(timestamp, out DateTime sentTime))
            {
                var responseTime = (DateTime.UtcNow - sentTime).TotalMilliseconds;
                _responseTimes.Add(responseTime);
            }
        });

        await connection.StartAsync();

        var stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < echoCount; i++)
        {
            await connection.InvokeAsync("Echo", $"Echo test {i}");
            await Task.Delay(10); // Small delay between messages
        }

        // Wait for all responses
        var timeout = TimeSpan.FromSeconds(30);
        var waitStart = DateTime.UtcNow;
        while (_responseTimes.Count < echoCount && DateTime.UtcNow - waitStart < timeout)
        {
            await Task.Delay(100);
        }

        stopwatch.Stop();

        Console.WriteLine($"\nEcho Response Time Test Results:");
        Console.WriteLine($"Total time: {stopwatch.Elapsed:mm\\:ss\\.fff}");
        Console.WriteLine($"Messages sent: {echoCount}");
        Console.WriteLine($"Responses received: {_responseTimes.Count}");
        
        if (_responseTimes.Count > 0)
        {
            var responseTimes = _responseTimes.ToArray();
            Array.Sort(responseTimes);
            
            Console.WriteLine($"Average response time: {responseTimes.Average():F2}ms");
            Console.WriteLine($"Min response time: {responseTimes.Min():F2}ms");
            Console.WriteLine($"Max response time: {responseTimes.Max():F2}ms");
            Console.WriteLine($"Median response time: {responseTimes[responseTimes.Length / 2]:F2}ms");
            Console.WriteLine($"95th percentile: {responseTimes[(int)(responseTimes.Length * 0.95)]:F2}ms");
            Console.WriteLine($"99th percentile: {responseTimes[(int)(responseTimes.Length * 0.99)]:F2}ms");
        }

        await connection.StopAsync();
    }

    private static async Task ConcurrentUsersTest(string serverUrl)
    {
        Console.Write("Enter number of concurrent users (default 50): ");
        var input = Console.ReadLine();
        int userCount = int.TryParse(input, out int result) ? result : 50;

        Console.Write("Enter messages per user (default 10): ");
        var msgInput = Console.ReadLine();
        int messagesPerUser = int.TryParse(msgInput, out int msgResult) ? msgResult : 10;

        Console.WriteLine($"\nStarting concurrent users test...");
        Console.WriteLine($"Users: {userCount}, Messages per user: {messagesPerUser}");

        var connections = new List<HubConnection>();
        var totalMessages = 0;
        var completedUsers = 0;

        // Create connections
        for (int i = 0; i < userCount; i++)
        {
            var connection = CreateConnection(serverUrl);
            connection.On<string, string>("ReceiveMessage", (connectionId, message) =>
            {
                Interlocked.Increment(ref totalMessages);
            });
            
            await connection.StartAsync();
            connections.Add(connection);
        }

        Console.WriteLine($"Created {connections.Count} user connections");

        var stopwatch = Stopwatch.StartNew();

        // Simulate concurrent users sending messages
        var userTasks = connections.Select(async (connection, index) =>
        {
            for (int i = 0; i < messagesPerUser; i++)
            {
                await connection.InvokeAsync("SendMessage", $"User {index} message {i}");
                await Task.Delay(Random.Shared.Next(100, 1000)); // Random delay between messages
            }
            Interlocked.Increment(ref completedUsers);
        });

        await Task.WhenAll(userTasks);
        stopwatch.Stop();

        Console.WriteLine($"\nConcurrent Users Test Results:");
        Console.WriteLine($"Total time: {stopwatch.Elapsed:mm\\:ss\\.fff}");
        Console.WriteLine($"Concurrent users: {userCount}");
        Console.WriteLine($"Messages per user: {messagesPerUser}");
        Console.WriteLine($"Total messages sent: {userCount * messagesPerUser}");
        Console.WriteLine($"Total messages received: {totalMessages}");
        Console.WriteLine($"Completed users: {completedUsers}");
        Console.WriteLine($"Messages per second: {totalMessages / stopwatch.Elapsed.TotalSeconds:F2}");

        // Cleanup
        foreach (var connection in connections)
        {
            await connection.StopAsync();
        }
    }

    private static async Task StressTest(string serverUrl)
    {
        Console.Write("Enter number of connections for stress test (default 20): ");
        var input = Console.ReadLine();
        int connectionCount = int.TryParse(input, out int result) ? result : 20;

        Console.Write("Enter test duration in seconds (default 60): ");
        var durationInput = Console.ReadLine();
        int duration = int.TryParse(durationInput, out int durationResult) ? durationResult : 60;

        Console.WriteLine($"\nStarting stress test...");
        Console.WriteLine($"Connections: {connectionCount}, Duration: {duration} seconds");

        var connections = new List<HubConnection>();
        var messagesSent = 0;
        var messagesReceived = 0;
        var errors = 0;

        // Create connections
        for (int i = 0; i < connectionCount; i++)
        {
            var connection = CreateConnection(serverUrl);
            connection.On<string, string>("ReceiveMessage", (connectionId, message) =>
            {
                Interlocked.Increment(ref messagesReceived);
            });
            
            await connection.StartAsync();
            connections.Add(connection);
        }

        Console.WriteLine($"Created {connections.Count} connections");

        var stopwatch = Stopwatch.StartNew();
        var cancellationTokenSource = new CancellationTokenSource();

        // Start stress test tasks
        var stressTasks = connections.Select(async connection =>
        {
            var random = new Random();
            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    await connection.InvokeAsync("SendMessage", $"Stress message {messagesSent}");
                    Interlocked.Increment(ref messagesSent);
                    await Task.Delay(random.Next(50, 200), cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception)
                {
                    Interlocked.Increment(ref errors);
                }
            }
        });

        // Run for specified duration
        await Task.Delay(TimeSpan.FromSeconds(duration));
        cancellationTokenSource.Cancel();

        await Task.WhenAll(stressTasks);
        stopwatch.Stop();

        Console.WriteLine($"\nStress Test Results:");
        Console.WriteLine($"Test duration: {stopwatch.Elapsed:mm\\:ss\\.fff}");
        Console.WriteLine($"Connections: {connectionCount}");
        Console.WriteLine($"Messages sent: {messagesSent}");
        Console.WriteLine($"Messages received: {messagesReceived}");
        Console.WriteLine($"Errors: {errors}");
        Console.WriteLine($"Success rate: {(messagesReceived / (double)messagesSent) * 100:F2}%");
        Console.WriteLine($"Messages per second: {messagesSent / stopwatch.Elapsed.TotalSeconds:F2}");
        Console.WriteLine($"Average per connection: {messagesSent / (double)connectionCount:F2} messages");

        // Cleanup
        foreach (var connection in connections)
        {
            await connection.StopAsync();
        }
    }

    private static void ShowStatistics()
    {
        Console.WriteLine("\n=== Performance Statistics ===");
        Console.WriteLine($"Active connections: {_connections.Count}");
        Console.WriteLine($"Total messages received: {_messagesReceived}");
        
        if (_responseTimes.Count > 0)
        {
            var responseTimes = _responseTimes.ToArray();
            Array.Sort(responseTimes);
            
            Console.WriteLine($"Response time samples: {responseTimes.Length}");
            Console.WriteLine($"Average response time: {responseTimes.Average():F2}ms");
            Console.WriteLine($"Min response time: {responseTimes.Min():F2}ms");
            Console.WriteLine($"Max response time: {responseTimes.Max():F2}ms");
            Console.WriteLine($"Median response time: {responseTimes[responseTimes.Length / 2]:F2}ms");
            Console.WriteLine($"95th percentile: {responseTimes[(int)(responseTimes.Length * 0.95)]:F2}ms");
            Console.WriteLine($"99th percentile: {responseTimes[(int)(responseTimes.Length * 0.99)]:F2}ms");
        }

        if (_connectionTimes.Count > 0)
        {
            Console.WriteLine($"Connection time samples: {_connectionTimes.Count}");
            Console.WriteLine($"Average connection time: {_connectionTimes.Average(t => t.TotalMilliseconds):F2}ms");
            Console.WriteLine($"Min connection time: {_connectionTimes.Min(t => t.TotalMilliseconds):F2}ms");
            Console.WriteLine($"Max connection time: {_connectionTimes.Max(t => t.TotalMilliseconds):F2}ms");
        }
    }

    private static async Task CleanupConnections()
    {
        Console.WriteLine("Cleaning up connections...");
        
        var connections = _connections.ToArray();
        var tasks = connections.Select(async connection =>
        {
            try
            {
                await connection.StopAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping connection: {ex.Message}");
            }
        });

        await Task.WhenAll(tasks);
        
        while (_connections.TryTake(out _)) { }
        
        Console.WriteLine($"Cleaned up {connections.Length} connections");
    }

    private static HubConnection CreateConnection(string serverUrl)
    {
        return new HubConnectionBuilder()
            .WithUrl(serverUrl)
            .WithAutomaticReconnect()
            .Build();
    }
}
