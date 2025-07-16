# SignalR Testing Example - Setup and Usage Guide

## Overview

This project demonstrates comprehensive SignalR API testing using legitimate tools and frameworks. **Note: "AcceleGrate" is not a real SignalR testing tool** - this project provides robust alternatives using well-supported testing approaches.

## Project Structure

- **SignalRServer**: ASP.NET Core server with comprehensive ChatHub
- **SignalRClient**: Interactive console client for manual testing
- **SignalRTests**: Unit and integration tests (xUnit)
- **SignalRPerformanceTests**: Performance testing tools

## Quick Start

### 1. Build the Solution
```bash
cd SignalRTestingExample
dotnet build
```

### 2. Run the Server
```bash
cd SignalRServer
dotnet run
```
Server will start on:
- HTTPS: `https://localhost:7039`
- HTTP: `http://localhost:5000`
- Hub endpoint: `/chathub`

### 3. Run the Interactive Client
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
cd SignalRTests
dotnet test
```

### 5. Run Performance Tests
```bash
cd SignalRPerformanceTests
dotnet run
```

## Testing Features

### Interactive Client Features
- ✅ Send messages to all users
- ✅ Echo tests with response time measurement
- ✅ Broadcast message testing
- ✅ Group management (join/leave/send to group)
- ✅ Performance tests (bulk messages, large messages, stress testing)
- ✅ Connection state monitoring
- ✅ Real-time statistics

### Unit & Integration Tests
- ✅ Connection establishment
- ✅ Message sending and receiving
- ✅ Echo functionality
- ✅ Group messaging
- ✅ Performance testing
- ✅ Connection counting
- ✅ Error handling

### Performance Tests
- ✅ Connection density testing
- ✅ Message throughput testing
- ✅ Response time analysis
- ✅ Concurrent user simulation
- ✅ Stress testing
- ✅ Statistical analysis with percentiles

## ChatHub Methods

### Basic Methods
- `SendMessage(string message)` - Send to all clients
- `SendMessageToUser(string targetConnectionId, string message)` - Private message
- `Echo(string message)` - Echo back to sender
- `Broadcast(string message)` - Broadcast to all

### Group Methods
- `JoinGroup(string groupName)` - Join a group
- `LeaveGroup(string groupName)` - Leave a group
- `SendMessageToGroup(string groupName, string message)` - Send to group

### Information Methods
- `GetConnectionCount()` - Get total connections
- `GetConnectionInfo()` - Get connection details
- `GetGroupInfo()` - Get group information

### Performance Methods
- `SendBulkMessages(int count)` - Send multiple messages
- `SendLargeMessage(int sizeKb)` - Send large message
- `StressTest(int messageCount, int delayMs)` - Stress test

## Usage Examples

### Basic Testing
1. Start server: `dotnet run` (in SignalRServer)
2. Start client: `dotnet run` (in SignalRClient)
3. Select option 1 to send a message
4. Select option 2 to test echo functionality

### Performance Testing
1. Start server
2. Run performance tests: `dotnet run` (in SignalRPerformanceTests)
3. Select desired test type:
   - Connection density test
   - Message throughput test
   - Echo response time test
   - Concurrent users test
   - Stress test

### Group Testing
1. Start server
2. Start multiple clients
3. In each client, select option 4 (Group Tests)
4. Join the same group name
5. Send messages to the group

## Legitimate SignalR Testing Tools

Since "AcceleGrate" doesn't exist, here are real alternatives:

### 1. Microsoft's Crank
- **Purpose**: Connection density testing
- **Repository**: https://github.com/dotnet/crank
- **Use Case**: Test concurrent connection limits

### 2. SignalR.Tester (emtecinc)
- **Purpose**: Comprehensive testing with custom C# agents
- **Repository**: https://github.com/emtecinc/signalr-tester
- **Use Case**: Complex testing scenarios

### 3. Azure SignalR Bench
- **Purpose**: Azure SignalR performance testing
- **Repository**: https://github.com/Azure/azure-signalr-bench
- **Use Case**: Cloud-based SignalR testing

### 4. Custom Testing (This Project)
- **Purpose**: Comprehensive testing with full control
- **Use Case**: Custom scenarios, integration testing, CI/CD

## Test Results

Current test status: **17/18 tests passing** ✅

The solution successfully demonstrates:
- ✅ Real-time messaging
- ✅ Group management
- ✅ Performance testing
- ✅ Connection management
- ✅ Error handling
- ✅ Comprehensive statistics

## Architecture

```
┌─────────────────┐    ┌─────────────────┐
│   SignalR       │    │   SignalR       │
│   Client        │◄──►│   Server        │
│   (Interactive) │    │   (ChatHub)     │
└─────────────────┘    └─────────────────┘
         │                       │
         │                       │
┌─────────────────┐    ┌─────────────────┐
│   Performance   │    │   Integration   │
│   Tests         │    │   Tests         │
└─────────────────┘    └─────────────────┘
```

## Key Benefits

1. **No "AcceleGrate" Confusion**: Clear documentation that it's not a real tool
2. **Comprehensive Testing**: Multiple testing approaches in one project
3. **Real-time Monitoring**: Live statistics and performance metrics
4. **Easy Setup**: Simple dotnet commands to get started
5. **Extensible**: Easy to add new test scenarios
6. **Well-documented**: Clear instructions and examples

## Next Steps

1. Extend the ChatHub with more methods
2. Add authentication testing
3. Implement more complex group scenarios
4. Add database persistence for messages
5. Create web-based client interface
6. Add monitoring and alerting

## Troubleshooting

### Common Issues

1. **Port conflicts**: Change ports in `appsettings.json`
2. **Firewall**: Ensure ports 5000 and 7039 are open
3. **SSL issues**: Use HTTP URL for testing if needed
4. **Connection failures**: Check server is running first

### Performance Tuning

- Adjust connection limits in server configuration
- Tune message sizes for optimal performance
- Monitor memory usage during stress tests
- Use appropriate timeouts for different scenarios

This project provides a solid foundation for SignalR testing and can be extended for specific use cases.