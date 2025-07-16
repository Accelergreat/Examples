# SignalR Testing Example Project

## Overview

This is a comprehensive example project demonstrating various approaches to testing SignalR APIs and real-time communication. The project includes a SignalR server with a feature-rich hub, multiple test clients, unit tests, integration tests, and performance testing tools.

## ⚠️ Important Note about "AcceleGrate"

**AcceleGrate is not a real SignalR testing tool.** Based on our research, no such tool exists in the SignalR ecosystem. This project provides comprehensive alternatives using legitimate, well-supported testing frameworks and approaches.

## Project Structure

```
SignalRTestingExample/
├── SignalRServer/              # SignalR Server with ChatHub
├── SignalRClient/              # Interactive test client
├── SignalRTests/               # Unit and integration tests
├── SignalRPerformanceTests/    # Performance testing tools
└── README.md                   # This file
```

## Features

### SignalR Server (`SignalRServer`)
- **Comprehensive ChatHub** with multiple testing endpoints
- **Connection Management** - Track connections, groups, and user states
- **Real-time Messaging** - Send messages to all, specific users, or groups
- **Performance Testing Endpoints** - Bulk messages, large messages, stress testing
- **CORS Support** - Configured for cross-origin testing
- **Automatic Reconnection** - Built-in reconnection logic

### Test Client (`SignalRClient`)
- **Interactive Menu System** - Easy-to-use testing interface
- **Multiple Test Types**:
  - Basic messaging tests
  - Echo tests with response time measurement
  - Broadcast tests
  - Group management tests
  - Performance tests (bulk, large messages, stress testing)
  - Connection state tests
- **Real-time Statistics** - Response times, messages per second, connection info
- **Automatic Reconnection** - Handles connection drops gracefully

### Testing Features
- **Unit Tests** - Test individual hub methods
- **Integration Tests** - Test complete SignalR workflows
- **Performance Tests** - Load testing and benchmarking
- **Connection Tests** - Test connection persistence and reconnection
- **Group Tests** - Test group-based messaging

## Getting Started

### Prerequisites
- .NET 8.0 SDK or later
- Visual Studio 2022 or VS Code (optional)

### 1. Clone and Build
```bash
git clone <repository-url>
cd SignalRTestingExample
dotnet restore
dotnet build
```

### 2. Run the Server
```bash
cd SignalRServer
dotnet run
```
The server will start on `https://localhost:7039` and `http://localhost:5000`

### 3. Run the Test Client
```bash
cd SignalRClient
dotnet run
```

Or specify a custom server URL:
```bash
dotnet run https://localhost:7039/chathub
```

### 4. Run Tests
```bash
# Run unit tests
cd SignalRTests
dotnet test

# Run performance tests
cd SignalRPerformanceTests
dotnet run
```

## SignalR Hub Methods

The `ChatHub` includes these testable methods:

### Basic Methods
- `SendMessage(string message)` - Send message to all connected clients
- `SendMessageToUser(string targetConnectionId, string message)` - Send private message
- `Echo(string message)` - Echo message back to sender
- `Broadcast(string message)` - Broadcast message to all clients

### Group Methods
- `JoinGroup(string groupName)` - Join a group
- `LeaveGroup(string groupName)` - Leave a group
- `SendMessageToGroup(string groupName, string message)` - Send message to group

### Information Methods
- `GetConnectionCount()` - Get total connection count
- `GetConnectionInfo()` - Get current connection info
- `GetGroupInfo()` - Get group information

### Performance Testing Methods
- `SendBulkMessages(int count)` - Send multiple messages rapidly
- `SendLargeMessage(int sizeKb)` - Send large message
- `StressTest(int messageCount, int delayMs)` - Perform stress test

## Testing Scenarios

### 1. Basic Connectivity Test
```bash
# Start server
cd SignalRServer && dotnet run

# Start client
cd SignalRClient && dotnet run

# Select option 6 (Connection Tests) -> option 1 (Get Connection Count)
```

### 2. Message Round-trip Test
```bash
# In client menu:
# Select option 2 (Echo Test)
# Enter number of messages to test
# Observe response times and statistics
```

### 3. Performance Test
```bash
# In client menu:
# Select option 5 (Performance Tests)
# Try different test types:
#   - Bulk Message Test (option 1)
#   - Large Message Test (option 2)
#   - Stress Test (option 3)
```

### 4. Group Testing
```bash
# In client menu:
# Select option 4 (Group Tests)
# Join a group (option 1)
# Send messages to group (option 3)
# Start multiple clients to test group messaging
```

## Legitimate SignalR Testing Tools

Since "AcceleGrate" doesn't exist, here are **real** SignalR testing tools and approaches:

### 1. Microsoft's Crank
- **Purpose**: Connection density testing
- **Repository**: https://github.com/dotnet/crank
- **Use Case**: Test how many concurrent connections your server can handle

### 2. SignalR.Tester (emtecinc)
- **Purpose**: Comprehensive SignalR testing
- **Repository**: https://github.com/emtecinc/signalr-tester
- **Use Case**: C#-based testing with custom agents

### 3. Azure SignalR Bench
- **Purpose**: Azure SignalR performance testing
- **Repository**: https://github.com/Azure/azure-signalr-bench
- **Use Case**: Benchmark Azure SignalR services

### 4. Crankier (Standalone)
- **Purpose**: Connection density testing
- **Repository**: https://github.com/mockjv/Crankier-and-Alone
- **Use Case**: Standalone version of Microsoft's testing tool

### 5. Custom Unit Testing
- **Framework**: xUnit, NUnit, MSTest
- **Libraries**: Microsoft.AspNetCore.SignalR.Test
- **Use Case**: Unit testing SignalR hubs

## Testing Best Practices

### 1. Unit Testing
```csharp
[Fact]
public async Task SendMessage_ShouldCallAllClients()
{
    // Test hub methods in isolation
    var hub = new ChatHub();
    var mockClients = new Mock<IHubCallerClients>();
    // ... test implementation
}
```

### 2. Integration Testing
```csharp
[Fact]
public async Task FullWorkflow_ShouldWork()
{
    // Test complete SignalR workflows
    var connection = new HubConnectionBuilder()
        .WithUrl("https://localhost:7039/chathub")
        .Build();
    
    await connection.StartAsync();
    // ... test workflow
}
```

### 3. Performance Testing
- **Connection Density**: Test maximum concurrent connections
- **Message Throughput**: Test messages per second
- **Memory Usage**: Monitor server memory consumption
- **Response Times**: Measure round-trip times

### 4. Reconnection Testing
- **Network Interruption**: Test connection drops
- **Server Restart**: Test client behavior during server restart
- **Graceful Degradation**: Test partial connectivity scenarios

## Performance Benchmarks

The included performance tests can measure:

- **Connection establishment time**
- **Message round-trip time**
- **Messages per second throughput**
- **Memory usage under load**
- **Connection density limits**

## Monitoring and Debugging

### 1. Enable Logging
```csharp
// In server
builder.Services.AddLogging(builder => builder.AddConsole());

// In client
.ConfigureLogging(logging =>
{
    logging.SetMinimumLevel(LogLevel.Information);
    logging.AddConsole();
})
```

### 2. Performance Counters
- Monitor CPU usage
- Track memory consumption
- Measure network throughput
- Count active connections

### 3. Application Insights
- For production monitoring
- Real-time metrics
- Error tracking
- Performance insights

## Common Testing Scenarios

### 1. Load Testing
```bash
# Test server capacity
# Run multiple clients simultaneously
# Monitor server performance
```

### 2. Stress Testing
```bash
# Push server beyond normal limits
# Test failure modes
# Verify graceful degradation
```

### 3. Reliability Testing
```bash
# Test reconnection logic
# Simulate network failures
# Test message delivery guarantees
```

### 4. Security Testing
```bash
# Test authentication
# Test authorization
# Test input validation
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For questions about SignalR testing:
- Check the official SignalR documentation
- Visit the ASP.NET Core GitHub repository
- Join the .NET community forums

## Conclusion

This project provides a comprehensive foundation for testing SignalR applications. While "AcceleGrate" isn't a real tool, the testing approaches and tools demonstrated here offer robust alternatives for validating SignalR implementations in production environments.
