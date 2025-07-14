# Basic SQLite Entity Framework Example

This example demonstrates the fundamentals of using Accelergreat with SQLite and Entity Framework Core for testing.

## What This Example Shows

- **Basic Entity Framework setup** with SQLite database
- **Simple CRUD operations** (Create, Read, Update, Delete)
- **Entity relationships** between Blog and Post entities
- **Database seeding** with initial test data
- **Parallel test execution** - each test gets its own isolated database
- **Transaction-based resets** for lightning-fast test execution

## Key Files

- `accelergreat.json` - Configuration file specifying SQLite settings
- `Models/` - Entity classes (Blog, Post) and DbContext
- `Components/BloggingDatabaseComponent.cs` - Accelergreat database component
- `Startup.cs` - Configuration of Accelergreat components
- `Tests/` - Test classes demonstrating CRUD operations

## How It Works

1. **Database Isolation**: Each test gets its own SQLite database file
2. **Seeded Data**: The `BloggingDatabaseComponent` seeds a "Tech Blog" for testing
3. **Transaction Resets**: Between tests, the database is reset using transactions (0-3ms)
4. **Parallel Execution**: Tests can run simultaneously without interference

## Running the Example

```bash
# Navigate to the example directory
cd 01-basic-sqlite-entity-framework

# Restore packages
dotnet restore

# Run the tests
dotnet test

# Run with verbose output
dotnet test --verbosity normal
```

## Key Concepts Demonstrated

### Database Component
The `BloggingDatabaseComponent` extends `SqliteEntityFrameworkDatabaseComponent<T>` and:
- Configures the DbContext for testing
- Seeds initial data via `OnDatabaseInitializedAsync`
- Handles database lifecycle management

### Test Structure
Each test class:
- Inherits from `AccelergreatXunitTest`
- Takes `IAccelergreatEnvironmentPool` in constructor
- Gets components via `GetComponent<T>()`
- Uses standard xUnit and FluentAssertions patterns

### Configuration
The `accelergreat.json` file specifies:
- SQLite Entity Framework provider
- Transaction-based reset strategy
- No connection string needed (SQLite creates temporary files)

## Expected Test Results

All tests should pass, demonstrating:
- ✅ Blog creation and retrieval
- ✅ Post creation with blog relationships
- ✅ Update and delete operations
- ✅ Parallel execution without conflicts
- ✅ Consistent seed data across tests

## Next Steps

After understanding this example, explore:
- `02-sqlserver-entity-framework` for SQL Server with migrations
- `03-aspnet-api-testing` for web API testing
- `06-combined-example` for complex scenarios

## Troubleshooting

**Q: Tests are failing with database errors**
A: Ensure you have the correct Accelergreat.EntityFramework.Sqlite package reference

**Q: Slow test execution**
A: SQLite with transactions should be very fast (< 50ms per test). Check your configuration.

**Q: Parallel execution issues**
A: Each test gets its own isolated database - conflicts suggest a configuration issue. 