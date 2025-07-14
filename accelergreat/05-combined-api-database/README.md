# Combined API + Database Testing Example

This example demonstrates the most comprehensive Accelergreat testing scenario: a complete REST API with Entity Framework database integration. It combines HTTP endpoint testing with database operations, transactions, and data isolation.

## What This Example Shows

- **Full-Stack API Testing** - REST endpoints with database persistence
- **Entity Framework Integration** - Complex data models with relationships
- **Database Isolation** - Each test gets its own database instance
- **CRUD Operations** - Create, Read, Update, Delete with database verification
- **Complex Data Relationships** - Projects, Tasks, and Comments with foreign keys
- **Transaction Testing** - Database operations with proper rollback
- **Seeded Data Testing** - Working with initial database state
- **API + Database Verification** - Ensuring API actions persist to database

## Project Structure

```
05-combined-api-database/
├── TaskApi/                          # ASP.NET Core API Project
│   ├── Controllers/
│   │   └── TasksController.cs        # REST API endpoints
│   ├── Data/
│   │   └── TaskDbContext.cs          # Entity Framework DbContext
│   ├── Models/
│   │   ├── TaskItem.cs               # Main task entity
│   │   ├── Project.cs                # Project entity
│   │   └── TaskComment.cs            # Comment entity
│   ├── Program.cs                    # API startup
│   └── TaskApi.csproj               # API project file
├── Components/                       # Accelergreat Components
│   ├── TaskApiComponent.cs          # Web API component
│   └── TaskDatabaseComponent.cs     # Database component
├── Tests/
│   └── TaskApiTests.cs              # Comprehensive API+DB tests
├── accelergreat.json               # Accelergreat configuration
├── Startup.cs                      # Test startup configuration
├── CombinedExample.csproj          # Test project file
└── README.md                       # This file
```

## Database Schema

### TaskItem (Main Entity)
- `Id` (Primary Key)
- `Title` (Required)
- `Description`
- `IsCompleted`
- `CreatedAt`
- `CompletedAt`
- `Priority` (Enum: Low, Medium, High, Critical)
- `AssignedTo`
- `ProjectId` (Foreign Key)

### Project
- `Id` (Primary Key)
- `Name` (Required)
- `Description`
- `CreatedAt`
- `CreatedBy`
- `IsActive`

### TaskComment
- `Id` (Primary Key)
- `Content` (Required)
- `CreatedAt`
- `CreatedBy`
- `TaskId` (Foreign Key)

### Relationships
- Project → Tasks (One-to-Many)
- Task → Comments (One-to-Many)

## API Endpoints

### Tasks Controller
- `GET /api/tasks` - Get all tasks with projects and comments
- `GET /api/tasks/{id}` - Get specific task with relationships
- `POST /api/tasks` - Create new task
- `PUT /api/tasks/{id}` - Update existing task
- `DELETE /api/tasks/{id}` - Delete task
- `POST /api/tasks/{id}/comments` - Add comment to task

## Key Features Demonstrated

### 1. Database Isolation
Each test gets its own SQL Server database instance:
```csharp
[Fact]
public async Task DatabaseIsolation_ShouldEnsureTestsAreIsolated()
{
    // Each test works with completely isolated data
    var uniqueTask = new TaskItem { Title = "Unique Task for Isolation Test" };
    // This task won't be visible in other tests
}
```

### 2. API + Database Verification
Tests verify both API responses and database state:
```csharp
[Fact]
public async Task CreateTask_ShouldCreateTaskInDatabase()
{
    // Test API call
    var response = await client.PostAsync("/api/tasks", content);
    response.StatusCode.Should().Be(HttpStatusCode.Created);
    
    // Verify database state
    using var context = databaseComponent.CreateDbContext();
    var savedTask = await context.Tasks.FirstOrDefaultAsync(t => t.Id == createdTask.Id);
    savedTask.Should().NotBeNull();
}
```

### 3. Complex Data Relationships
```csharp
[Fact]
public async Task GetTask_WithComments_ShouldReturnTaskWithComments()
{
    // Tests that related data (comments, projects) are properly loaded
    var task = JsonConvert.DeserializeObject<TaskItem>(content);
    task.Comments.Should().NotBeEmpty();
    task.Project.Should().NotBeNull();
}
```

### 4. Seeded Data Testing
The database starts with predefined data:
```csharp
[Fact]
public async Task GetTasks_ShouldReturnSeededTasks()
{
    // Works with initial data defined in DbContext
    tasks.Should().HaveCount(2);
    tasks.Should().Contain(t => t.Title == "Setup project");
}
```

## Configuration

### accelergreat.json
```json
{
  "database": {
    "connectionString": "Server=(localdb)\\MSSQLLocalDB;Database=TaskApiTest_{EnvironmentId};Trusted_Connection=true;",
    "resetStrategy": "recreate"
  },
  "parallel": {
    "enabled": true,
    "maxConcurrency": 4
  }
}
```

### Components Integration
```csharp
public class Startup : IAccelergreatStartup
{
    public void Configure(IAccelergreatBuilder builder)
    {
        builder.AddAccelergreatComponent<TaskDatabaseComponent>();
        builder.AddAccelergreatComponent<TaskApiComponent>();
    }
}
```

## Running the Example

```bash
# Navigate to the example directory
cd 05-combined-api-database

# Restore packages
dotnet restore

# Run the tests
dotnet test

# Run with detailed output
dotnet test --verbosity normal
```

## Test Coverage

### API Endpoint Tests (7 tests)
- ✅ Get all tasks with relationships
- ✅ Create task via API + verify database
- ✅ Update task via API + verify database
- ✅ Delete task via API + verify database
- ✅ Add comments to tasks
- ✅ Get task with comments and project
- ✅ Database isolation verification

### Database Operations Verified
- ✅ Entity creation and persistence
- ✅ Entity updates and change tracking
- ✅ Entity deletion and cleanup
- ✅ Relationship loading (Include queries)
- ✅ Foreign key constraints
- ✅ Database seeding and initial state

## Performance Characteristics

- **Test Execution**: ~200-500ms per test (includes database setup)
- **Database Reset**: Fast with `recreate` strategy
- **Parallel Execution**: Full support with isolated databases
- **Memory Usage**: Moderate (SQL Server LocalDB instances)

## Accelergreat Benefits Demonstrated

### 1. **Complete Environment Isolation**
- Each test gets its own database
- No test interference or data pollution
- Clean state for every test run

### 2. **Integrated Testing**
- Tests the complete request-response cycle
- Verifies both API behavior and data persistence
- Realistic end-to-end scenarios

### 3. **Performance Optimization**
- Parallel test execution
- Fast database reset strategies
- Efficient resource management

### 4. **Complex Scenario Support**
- Multi-entity relationships
- Transaction handling
- Data seeding and migration

## Common Patterns

### Test Structure
```csharp
public class TaskApiTests : AccelergreatXunitTest
{
    public TaskApiTests(IAccelergreatEnvironmentPool environmentPool) 
        : base(environmentPool) { }

    [Fact]
    public async Task TestMethod()
    {
        // Arrange - Get components
        var webAppComponent = GetComponent<TaskApiComponent>();
        var databaseComponent = GetComponent<TaskDatabaseComponent>();
        var client = webAppComponent.CreateClient();

        // Act - API call
        var response = await client.PostAsync("/api/tasks", content);

        // Assert - API response
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        // Assert - Database state
        using var context = databaseComponent.CreateDbContext();
        var savedEntity = await context.Tasks.FirstOrDefaultAsync(t => t.Id == id);
        savedEntity.Should().NotBeNull();
    }
}
```

### Database Context Usage
```csharp
using var context = databaseComponent.CreateDbContext();
var entities = await context.Tasks
    .Include(t => t.Project)
    .Include(t => t.Comments)
    .ToListAsync();
```

### API Testing with JSON
```csharp
var entity = new TaskItem { Title = "Test Task" };
var json = JsonConvert.SerializeObject(entity);
var content = new StringContent(json, Encoding.UTF8, "application/json");
var response = await client.PostAsync("/api/tasks", content);
```

## Best Practices Demonstrated

1. **Comprehensive Verification** - Test both API and database state
2. **Relationship Testing** - Verify complex entity relationships
3. **Isolation Verification** - Ensure tests don't interfere
4. **Realistic Data** - Use meaningful test data
5. **Error Scenarios** - Test validation and error handling
6. **Performance Awareness** - Efficient database queries
7. **Clean Architecture** - Separation of concerns

## Troubleshooting

**Q: Tests fail with database connection errors**
A: Ensure SQL Server LocalDB is installed and running

**Q: Entity relationship errors**
A: Check that Include() statements are properly configured

**Q: JSON serialization issues**
A: Verify model properties match API expectations

**Q: Database isolation not working**
A: Check that each test uses unique data identifiers

**Q: Performance issues**
A: Consider using transactions instead of database recreation for faster tests

## Next Steps

This example demonstrates the most advanced Accelergreat testing scenario. From here, you can:
- Add more complex business logic
- Implement authentication and authorization testing
- Add integration with external services
- Create performance benchmarks
- Implement advanced Entity Framework features

## Advanced Features

- **Audit Trails** - Track entity changes
- **Soft Deletes** - Implement IsDeleted patterns
- **Bulk Operations** - Test bulk inserts/updates
- **Stored Procedures** - Call database procedures
- **Views and Functions** - Test complex database logic
- **Migrations** - Test database schema changes 