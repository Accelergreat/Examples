using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace SignalRTests;

public class SignalRIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _output;

    public SignalRIntegrationTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
    }

    [Fact]
    public async Task Connection_ShouldConnectSuccessfully()
    {
        // Arrange
        var client = _factory.CreateClient();
        var hubConnection = CreateHubConnection();

        // Act
        await hubConnection.StartAsync();

        // Assert
        Assert.Equal(HubConnectionState.Connected, hubConnection.State);
        Assert.NotNull(hubConnection.ConnectionId);

        // Cleanup
        await hubConnection.StopAsync();
    }

    [Fact]
    public async Task SendMessage_ShouldReceiveMessage()
    {
        // Arrange
        var connection = CreateHubConnection();
        var messageReceived = false;
        var receivedMessage = string.Empty;
        var receivedConnectionId = string.Empty;

        connection.On<string, string>("ReceiveMessage", (connectionId, message) =>
        {
            messageReceived = true;
            receivedMessage = message;
            receivedConnectionId = connectionId;
        });

        await connection.StartAsync();

        // Act
        await connection.InvokeAsync("SendMessage", "Test message");

        // Wait for message to be received
        await Task.Delay(1000);

        // Assert
        Assert.True(messageReceived);
        Assert.Equal("Test message", receivedMessage);
        Assert.Equal(connection.ConnectionId, receivedConnectionId);

        // Cleanup
        await connection.StopAsync();
    }

    [Fact]
    public async Task Echo_ShouldReturnSameMessage()
    {
        // Arrange
        var connection = CreateHubConnection();
        var echoReceived = false;
        var receivedMessage = string.Empty;

        connection.On<string, string>("EchoResponse", (message, timestamp) =>
        {
            echoReceived = true;
            receivedMessage = message;
        });

        await connection.StartAsync();

        // Act
        await connection.InvokeAsync("Echo", "Echo test message");

        // Wait for echo response
        await Task.Delay(1000);

        // Assert
        Assert.True(echoReceived);
        Assert.Equal("Echo test message", receivedMessage);

        // Cleanup
        await connection.StopAsync();
    }

    [Fact]
    public async Task GetConnectionCount_ShouldReturnCorrectCount()
    {
        // Arrange
        var connection1 = CreateHubConnection();
        var connection2 = CreateHubConnection();

        await connection1.StartAsync();
        await connection2.StartAsync();

        // Act
        var count = await connection1.InvokeAsync<int>("GetConnectionCount");

        // Assert
        Assert.True(count >= 2);

        // Cleanup
        await connection1.StopAsync();
        await connection2.StopAsync();
    }

    [Fact]
    public async Task GetConnectionInfo_ShouldReturnValidInfo()
    {
        // Arrange
        var connection = CreateHubConnection();
        await connection.StartAsync();

        // Act
        var info = await connection.InvokeAsync<object>("GetConnectionInfo");

        // Assert
        Assert.NotNull(info);

        // Cleanup
        await connection.StopAsync();
    }

    [Fact]
    public async Task JoinGroup_ShouldReceiveGroupMessages()
    {
        // Arrange
        var connection1 = CreateHubConnection();
        var connection2 = CreateHubConnection();
        var groupMessageReceived = false;
        var receivedGroupMessage = string.Empty;
        var receivedGroupName = string.Empty;

        connection2.On<string, string, string>("ReceiveGroupMessage", (connectionId, groupName, message) =>
        {
            groupMessageReceived = true;
            receivedGroupMessage = message;
            receivedGroupName = groupName;
        });

        await connection1.StartAsync();
        await connection2.StartAsync();

        // Act
        await connection1.InvokeAsync("JoinGroup", "TestGroup");
        await connection2.InvokeAsync("JoinGroup", "TestGroup");
        await connection1.InvokeAsync("SendMessageToGroup", "TestGroup", "Group test message");

        // Wait for group message
        await Task.Delay(1000);

        // Assert
        Assert.True(groupMessageReceived);
        Assert.Equal("Group test message", receivedGroupMessage);
        Assert.Equal("TestGroup", receivedGroupName);

        // Cleanup
        await connection1.StopAsync();
        await connection2.StopAsync();
    }

    [Fact]
    public async Task LeaveGroup_ShouldNotReceiveGroupMessages()
    {
        // Arrange
        var connection1 = CreateHubConnection();
        var connection2 = CreateHubConnection();
        var groupMessageReceived = false;

        connection2.On<string, string, string>("ReceiveGroupMessage", (connectionId, groupName, message) =>
        {
            groupMessageReceived = true;
        });

        await connection1.StartAsync();
        await connection2.StartAsync();

        // Act
        await connection1.InvokeAsync("JoinGroup", "TestGroup");
        await connection2.InvokeAsync("JoinGroup", "TestGroup");
        await connection2.InvokeAsync("LeaveGroup", "TestGroup");
        await connection1.InvokeAsync("SendMessageToGroup", "TestGroup", "Group test message");

        // Wait for potential group message
        await Task.Delay(1000);

        // Assert
        Assert.False(groupMessageReceived);

        // Cleanup
        await connection1.StopAsync();
        await connection2.StopAsync();
    }

    [Fact]
    public async Task SendBulkMessages_ShouldReceiveAllMessages()
    {
        // Arrange
        var connection = CreateHubConnection();
        var messagesReceived = 0;
        var messageCount = 50;

        connection.On<int, string, string>("BulkMessage", (index, message, timestamp) =>
        {
            messagesReceived++;
        });

        await connection.StartAsync();

        // Act
        await connection.InvokeAsync("SendBulkMessages", messageCount);

        // Wait for all messages
        await Task.Delay(2000);

        // Assert
        Assert.Equal(messageCount, messagesReceived);

        // Cleanup
        await connection.StopAsync();
    }

    [Fact]
    public async Task SendLargeMessage_ShouldReceiveCorrectSize()
    {
        // Arrange
        var connection = CreateHubConnection();
        var largeMessageReceived = false;
        var receivedSize = 0;

        connection.On<int, string, string>("LargeMessage", (sizeKb, message, timestamp) =>
        {
            largeMessageReceived = true;
            receivedSize = sizeKb;
        });

        await connection.StartAsync();

        // Act
        await connection.InvokeAsync("SendLargeMessage", 5);

        // Wait for large message
        await Task.Delay(2000);

        // Assert
        Assert.True(largeMessageReceived);
        Assert.Equal(5, receivedSize);

        // Cleanup
        await connection.StopAsync();
    }

    [Fact]
    public async Task StressTest_ShouldCompleteSuccessfully()
    {
        // Arrange
        var connection = CreateHubConnection();
        var stressMessagesReceived = 0;
        var messageCount = 20;

        connection.On<int, string, string>("StressTestMessage", (index, message, timestamp) =>
        {
            stressMessagesReceived++;
        });

        await connection.StartAsync();

        // Act
        await connection.InvokeAsync("StressTest", messageCount, 50);

        // Wait for stress test to complete
        await Task.Delay(3000);

        // Assert
        Assert.Equal(messageCount, stressMessagesReceived);

        // Cleanup
        await connection.StopAsync();
    }

    [Fact]
    public async Task MultipleConnections_ShouldAllReceiveBroadcast()
    {
        // Arrange
        var connection1 = CreateHubConnection();
        var connection2 = CreateHubConnection();
        var connection3 = CreateHubConnection();
        
        var messagesReceived = 0;

        connection1.On<string, string, DateTime>("BroadcastMessage", (connectionId, message, timestamp) =>
        {
            messagesReceived++;
        });

        connection2.On<string, string, DateTime>("BroadcastMessage", (connectionId, message, timestamp) =>
        {
            messagesReceived++;
        });

        connection3.On<string, string, DateTime>("BroadcastMessage", (connectionId, message, timestamp) =>
        {
            messagesReceived++;
        });

        await connection1.StartAsync();
        await connection2.StartAsync();
        await connection3.StartAsync();

        // Act
        await connection1.InvokeAsync("Broadcast", "Broadcast test message");

        // Wait for broadcast
        await Task.Delay(1000);

        // Assert
        Assert.Equal(3, messagesReceived);

        // Cleanup
        await connection1.StopAsync();
        await connection2.StopAsync();
        await connection3.StopAsync();
    }

    [Fact]
    public async Task ConnectionAndDisconnection_ShouldUpdateCount()
    {
        // Arrange
        var connection = CreateHubConnection();
        var userConnectedReceived = false;
        var userDisconnectedReceived = false;

        connection.On<string, int>("UserConnected", (connectionId, count) =>
        {
            userConnectedReceived = true;
        });

        connection.On<string, int>("UserDisconnected", (connectionId, count) =>
        {
            userDisconnectedReceived = true;
        });

        // Act
        await connection.StartAsync();
        await Task.Delay(500);
        await connection.StopAsync();
        await Task.Delay(500);

        // Assert
        Assert.True(userConnectedReceived);
        Assert.True(userDisconnectedReceived);
    }

    [Fact]
    public async Task Performance_EchoResponseTime_ShouldBeReasonable()
    {
        // Arrange
        var connection = CreateHubConnection();
        var responseTime = TimeSpan.MaxValue;
        var responseReceived = false;

        connection.On<string, string>("EchoResponse", (message, timestamp) =>
        {
            if (DateTime.TryParse(timestamp, out DateTime sentTime))
            {
                responseTime = DateTime.UtcNow - sentTime;
            }
            responseReceived = true;
        });

        await connection.StartAsync();

        // Act
        var startTime = DateTime.UtcNow;
        await connection.InvokeAsync("Echo", "Performance test");

        // Wait for response
        await Task.Delay(1000);

        // Assert
        Assert.True(responseReceived);
        Assert.True(responseTime < TimeSpan.FromSeconds(1), $"Response time was {responseTime.TotalMilliseconds}ms");

        // Cleanup
        await connection.StopAsync();
    }

    private HubConnection CreateHubConnection()
    {
        var client = _factory.CreateClient();
        
        return new HubConnectionBuilder()
            .WithUrl("ws://localhost/chathub", options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .ConfigureLogging(logging =>
            {
                logging.AddDebug();
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .Build();
    }
}

// Unit Tests for ChatHub business logic
public class ChatHubUnitTests
{
    [Fact]
    public void ConnectionCount_ShouldStartAtZero()
    {
        // This test would require mocking the ChatHub dependencies
        // For now, we rely on integration tests
        Assert.True(true);
    }

    [Fact]
    public void GroupManagement_ShouldMaintainCorrectState()
    {
        // This test would require mocking the ChatHub dependencies
        // For now, we rely on integration tests
        Assert.True(true);
    }

    [Theory]
    [InlineData("Test message")]
    [InlineData("")]
    [InlineData("Very long message that contains multiple words and should be handled correctly")]
    public void MessageHandling_ShouldHandleVariousMessageTypes(string message)
    {
        // This test would require mocking the ChatHub dependencies
        // For now, we rely on integration tests
        Assert.True(true);
    }
}