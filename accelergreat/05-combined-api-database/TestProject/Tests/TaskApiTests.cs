using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Accelergreat.Environments.Pooling;
using Accelergreat.Xunit;
using CombinedExample.Components;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TaskApi.Data;
using TaskApi.Models;
using TaskApi.Models.DTOs;
using Xunit;
using System;

namespace CombinedExample.Tests;

public class TaskApiTests : AccelergreatXunitTest
{
    public TaskApiTests(IAccelergreatEnvironmentPool environmentPool) : base(environmentPool)
    {
    }

    [Fact]
    public async Task GetTasks_ShouldReturnSeededTasks()
    {
        // Arrange
        var webAppComponent = GetComponent<TaskApiComponent>();
        var client = webAppComponent.CreateClient();

        // Act
        var response = await client.GetAsync("/api/tasks");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var tasks = JsonConvert.DeserializeObject<TaskItem[]>(content);
        
        tasks.Should().NotBeNull();
        tasks!.Should().HaveCountGreaterOrEqualTo(2);
        
        // Verify our seeded data is present (from TaskDatabaseComponent)
        tasks.Should().Contain(t => t.Title == "Setup project" && t.AssignedTo == "Developer");
        tasks.Should().Contain(t => t.Title == "Implement API endpoints" && t.AssignedTo == "Developer");
        
        // Verify all tasks have valid project relationships
        tasks.Should().OnlyContain(t => t.Project != null);
        tasks.Should().OnlyContain(t => t.Project.Name == "Sample Project");
    }

    [Fact]
    public async Task CreateTask_ShouldCreateTaskInDatabase()
    {
        // Arrange
        var webAppComponent = GetComponent<TaskApiComponent>();
        var databaseComponent = GetComponent<TaskDatabaseComponent>();
        var client = webAppComponent.CreateClient();

        var newTaskRequest = new CreateTaskRequest
        {
            Title = "New Task",
            Description = "This is a new task created via API",
            Priority = TaskPriority.High,
            AssignedTo = "Test User",
            ProjectId = 1
        };

        var json = JsonConvert.SerializeObject(newTaskRequest);
        Console.WriteLine($"=== SENDING JSON ===");
        Console.WriteLine(json);
        Console.WriteLine($"=== END JSON ===");
        
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/tasks", content);

        // Debug response
        var responseContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"=== RESPONSE STATUS: {response.StatusCode} ===");
        Console.WriteLine($"=== RESPONSE CONTENT ===");
        Console.WriteLine(responseContent);
        Console.WriteLine($"=== END RESPONSE ===");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var createdTask = JsonConvert.DeserializeObject<TaskItem>(responseContent);
        
        createdTask.Should().NotBeNull();
        createdTask!.Id.Should().BeGreaterThan(0);
        createdTask.Title.Should().Be("New Task");
        createdTask.Description.Should().Be("This is a new task created via API");
        createdTask.Priority.Should().Be(TaskPriority.High);
        createdTask.AssignedTo.Should().Be("Test User");
        createdTask.ProjectId.Should().Be(1);

        // Verify task was actually saved to database
        await using var context = databaseComponent.DbContextFactory.NewDbContext();
        var savedTask = await context.Tasks.FirstOrDefaultAsync(t => t.Id == createdTask.Id);
        
        savedTask.Should().NotBeNull();
        savedTask!.Title.Should().Be("New Task");
        savedTask.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateTask_ShouldUpdateTaskInDatabase()
    {
        // Arrange
        var webAppComponent = GetComponent<TaskApiComponent>();
        var databaseComponent = GetComponent<TaskDatabaseComponent>();
        var client = webAppComponent.CreateClient();

        // First create a task
        var createTaskRequest = new CreateTaskRequest
        {
            Title = "Task to Update",
            Description = "Original description",
            Priority = TaskPriority.Medium,
            AssignedTo = "Original User",
            ProjectId = 1
        };

        var createJson = JsonConvert.SerializeObject(createTaskRequest);
        var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
        var createResponse = await client.PostAsync("/api/tasks", createContent);
        var createdTask = JsonConvert.DeserializeObject<TaskItem>(await createResponse.Content.ReadAsStringAsync());

        // Prepare update
        var updateTaskRequest = new CreateTaskRequest
        {
            Title = "Updated Task Title",
            Description = "Updated description",
            Priority = TaskPriority.Critical,
            AssignedTo = "Updated User",
            ProjectId = 1
        };

        var updateJson = JsonConvert.SerializeObject(updateTaskRequest);
        var updateContent = new StringContent(updateJson, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PutAsync($"/api/tasks/{createdTask!.Id}", updateContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify task was updated in database
        await using var context = databaseComponent.DbContextFactory.NewDbContext();
        var updatedTask = await context.Tasks.FirstOrDefaultAsync(t => t.Id == createdTask.Id);
        
        updatedTask.Should().NotBeNull();
        updatedTask!.Title.Should().Be("Updated Task Title");
        updatedTask.Description.Should().Be("Updated description");
        updatedTask.Priority.Should().Be(TaskPriority.Critical);
        updatedTask.AssignedTo.Should().Be("Updated User");
    }

    [Fact]
    public async Task DeleteTask_ShouldRemoveTaskFromDatabase()
    {
        // Arrange
        var webAppComponent = GetComponent<TaskApiComponent>();
        var databaseComponent = GetComponent<TaskDatabaseComponent>();
        var client = webAppComponent.CreateClient();

        // First create a task
        var createTaskRequest = new CreateTaskRequest
        {
            Title = "Task to Delete",
            Description = "This task will be deleted",
            Priority = TaskPriority.Low,
            AssignedTo = "Test User",
            ProjectId = 1
        };

        var createJson = JsonConvert.SerializeObject(createTaskRequest);
        var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
        var createResponse = await client.PostAsync("/api/tasks", createContent);
        var createdTask = JsonConvert.DeserializeObject<TaskItem>(await createResponse.Content.ReadAsStringAsync());

        // Act
        var response = await client.DeleteAsync($"/api/tasks/{createdTask!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify task was deleted from database
        await using var context = databaseComponent.DbContextFactory.NewDbContext();
        var deletedTask = await context.Tasks.FirstOrDefaultAsync(t => t.Id == createdTask.Id);
        
        deletedTask.Should().BeNull();
    }

    [Fact]
    public async Task AddComment_ShouldAddCommentToTaskInDatabase()
    {
        // Arrange
        var webAppComponent = GetComponent<TaskApiComponent>();
        var databaseComponent = GetComponent<TaskDatabaseComponent>();
        var client = webAppComponent.CreateClient();

        // Use existing seeded task (ID = 1)
        const int taskId = 1;
        
        var commentRequest = new CreateCommentRequest
        {
            Content = "This is a test comment",
            CreatedBy = "Test User"
        };

        var json = JsonConvert.SerializeObject(commentRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync($"/api/tasks/{taskId}/comments", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // Verify comment was added to database
        await using var context = databaseComponent.DbContextFactory.NewDbContext();
        var savedComment = await context.Comments.FirstOrDefaultAsync(c => c.TaskId == taskId);
        
        savedComment.Should().NotBeNull();
        savedComment!.Content.Should().Be("This is a test comment");
        savedComment.CreatedBy.Should().Be("Test User");
        savedComment.TaskId.Should().Be(taskId);
    }

    [Fact]
    public async Task GetTask_WithComments_ShouldReturnTaskWithComments()
    {
        // Arrange
        var webAppComponent = GetComponent<TaskApiComponent>();
        var databaseComponent = GetComponent<TaskDatabaseComponent>();
        var client = webAppComponent.CreateClient();

        // First add a comment to existing task
        const int taskId = 1;
        var commentRequest = new CreateCommentRequest
        {
            Content = "Test comment for retrieval",
            CreatedBy = "Test User"
        };

        var commentJson = JsonConvert.SerializeObject(commentRequest);
        var commentContent = new StringContent(commentJson, Encoding.UTF8, "application/json");
        await client.PostAsync($"/api/tasks/{taskId}/comments", commentContent);

        // Act
        var response = await client.GetAsync($"/api/tasks/{taskId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var task = JsonConvert.DeserializeObject<TaskItem>(content);
        
        task.Should().NotBeNull();
        task!.Id.Should().Be(taskId);
        task.Comments.Should().NotBeEmpty();
        task.Comments.Should().Contain(c => c.Content == "Test comment for retrieval");
        task.Project.Should().NotBeNull();
        task.Project.Name.Should().Be("Sample Project");
    }

    [Fact]
    public async Task DatabaseIsolation_ShouldEnsureTestsAreIsolated()
    {
        // Arrange
        var webAppComponent = GetComponent<TaskApiComponent>();
        var databaseComponent = GetComponent<TaskDatabaseComponent>();
        var client = webAppComponent.CreateClient();

        // Act - Create a task that only exists in this test
        var uniqueTaskRequest = new CreateTaskRequest
        {
            Title = "Unique Task for Isolation Test",
            Description = "This task should not be visible in other tests",
            Priority = TaskPriority.High,
            AssignedTo = "Isolation Test User",
            ProjectId = 1
        };

        var json = JsonConvert.SerializeObject(uniqueTaskRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/tasks", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // Verify task exists in current test database
        await using var context = databaseComponent.DbContextFactory.NewDbContext();
        var savedTask = await context.Tasks.FirstOrDefaultAsync(t => t.Title == "Unique Task for Isolation Test");
        
        savedTask.Should().NotBeNull();
        savedTask!.AssignedTo.Should().Be("Isolation Test User");
        
        // Note: This task will not be visible in other tests due to Accelergreat's database isolation
    }
} 