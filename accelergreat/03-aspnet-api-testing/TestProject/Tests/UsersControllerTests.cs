using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Accelergreat.Environments.Pooling;
using Accelergreat.Xunit;
using AspNetApiExample.Components;
using FluentAssertions;
using Newtonsoft.Json;
using TestApi.Models;
using Xunit;

namespace AspNetApiExample.Tests;

public class UsersControllerTests : AccelergreatXunitTest
{
    public UsersControllerTests(IAccelergreatEnvironmentPool environmentPool) : base(environmentPool)
    {
    }

    [Fact]
    public async Task GetAllUsers_ShouldReturnSeededUsers()
    {
        // Arrange
        var webAppComponent = GetComponent<TestApiWebAppComponent>();
        var client = webAppComponent.CreateClient();

        // Act
        var response = await client.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var users = JsonConvert.DeserializeObject<User[]>(content);

        users.Should().NotBeNull();
        users!.Should().HaveCount(3); // Seeded users
        users.Should().Contain(u => u.Name == "John Doe");
        users.Should().Contain(u => u.Name == "Jane Smith");
        users.Should().Contain(u => u.Name == "Bob Johnson");
        users.Should().OnlyContain(u => u.IsActive);
    }

    [Fact]
    public async Task GetUserById_WithValidId_ShouldReturnUser()
    {
        // Arrange
        var webAppComponent = GetComponent<TestApiWebAppComponent>();
        var client = webAppComponent.CreateClient();
        const int userId = 1;

        // Act
        var response = await client.GetAsync($"/api/users/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var user = JsonConvert.DeserializeObject<User>(content);

        user.Should().NotBeNull();
        user!.Id.Should().Be(userId);
        user.Name.Should().Be("John Doe");
        user.Email.Should().Be("john@example.com");
    }

    [Fact]
    public async Task GetUserById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var webAppComponent = GetComponent<TestApiWebAppComponent>();
        var client = webAppComponent.CreateClient();
        const int invalidUserId = 999;

        // Act
        var response = await client.GetAsync($"/api/users/{invalidUserId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateUser_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var webAppComponent = GetComponent<TestApiWebAppComponent>();
        var client = webAppComponent.CreateClient();

        var newUser = new User
        {
            Name = "Alice Cooper",
            Email = "alice@example.com"
        };

        var json = JsonConvert.SerializeObject(newUser);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/users", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseContent = await response.Content.ReadAsStringAsync();
        var createdUser = JsonConvert.DeserializeObject<User>(responseContent);

        createdUser.Should().NotBeNull();
        createdUser!.Name.Should().Be(newUser.Name);
        createdUser.Email.Should().Be(newUser.Email);
        createdUser.Id.Should().BeGreaterThan(0);
        createdUser.IsActive.Should().BeTrue();
        createdUser.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));

        // Verify Location header
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain($"/api/Users/{createdUser.Id}");
    }

    [Fact]
    public async Task CreateUser_WithMissingName_ShouldReturnBadRequest()
    {
        // Arrange
        var webAppComponent = GetComponent<TestApiWebAppComponent>();
        var client = webAppComponent.CreateClient();

        var invalidUser = new User
        {
            Email = "test@example.com"
            // Name is missing
        };

        var json = JsonConvert.SerializeObject(invalidUser);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/users", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateUser_WithMissingEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var webAppComponent = GetComponent<TestApiWebAppComponent>();
        var client = webAppComponent.CreateClient();

        var invalidUser = new User
        {
            Name = "Test User"
            // Email is missing
        };

        var json = JsonConvert.SerializeObject(invalidUser);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/users", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateUser_WithValidData_ShouldUpdateUser()
    {
        // Arrange
        var webAppComponent = GetComponent<TestApiWebAppComponent>();
        var client = webAppComponent.CreateClient();
        const int userId = 2;

        var updatedUser = new User
        {
            Name = "Jane Smith Updated",
            Email = "jane.updated@example.com"
        };

        var json = JsonConvert.SerializeObject(updatedUser);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PutAsync($"/api/users/{userId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var returnedUser = JsonConvert.DeserializeObject<User>(responseContent);

        returnedUser.Should().NotBeNull();
        returnedUser!.Id.Should().Be(userId);
        returnedUser.Name.Should().Be(updatedUser.Name);
        returnedUser.Email.Should().Be(updatedUser.Email);
    }

    [Fact]
    public async Task UpdateUser_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var webAppComponent = GetComponent<TestApiWebAppComponent>();
        var client = webAppComponent.CreateClient();
        const int invalidUserId = 999;

        var updatedUser = new User
        {
            Name = "Updated Name",
            Email = "updated@example.com"
        };

        var json = JsonConvert.SerializeObject(updatedUser);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PutAsync($"/api/users/{invalidUserId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteUser_WithValidId_ShouldDeleteUser()
    {
        // Arrange
        var webAppComponent = GetComponent<TestApiWebAppComponent>();
        var client = webAppComponent.CreateClient();
        const int userId = 3;

        // Act
        var response = await client.DeleteAsync($"/api/users/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify user is no longer accessible
        var getResponse = await client.GetAsync($"/api/users/{userId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteUser_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var webAppComponent = GetComponent<TestApiWebAppComponent>();
        var client = webAppComponent.CreateClient();
        const int invalidUserId = 999;

        // Act
        var response = await client.DeleteAsync($"/api/users/{invalidUserId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateAndRetrieveUser_FullWorkflow_ShouldWork()
    {
        // Arrange
        var webAppComponent = GetComponent<TestApiWebAppComponent>();
        var client = webAppComponent.CreateClient();

        var newUser = new User
        {
            Name = "Workflow Test User",
            Email = "workflow@example.com"
        };

        // Act 1: Create user
        var json = JsonConvert.SerializeObject(newUser);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var createResponse = await client.PostAsync("/api/users", content);

        // Assert 1: User created successfully
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdUser = JsonConvert.DeserializeObject<User>(await createResponse.Content.ReadAsStringAsync());

        // Act 2: Retrieve created user
        var getResponse = await client.GetAsync($"/api/users/{createdUser!.Id}");

        // Assert 2: Retrieved user matches created user
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var retrievedUser = JsonConvert.DeserializeObject<User>(await getResponse.Content.ReadAsStringAsync());

        retrievedUser.Should().NotBeNull();
        retrievedUser!.Id.Should().Be(createdUser.Id);
        retrievedUser.Name.Should().Be(newUser.Name);
        retrievedUser.Email.Should().Be(newUser.Email);
    }
} 