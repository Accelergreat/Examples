# SQL Server Entity Framework Example

This example demonstrates enterprise-level testing with SQL Server and Entity Framework Core, showcasing advanced Accelergreat features and complex data relationships.

## What This Example Shows

- **SQL Server integration** with LocalDB
- **Complex entity relationships** in an e-commerce domain
- **Advanced Entity Framework features** (indexes, constraints, decimal columns)
- **Multiple reset strategies** (Transactions vs Snapshot Rollback)
- **Comprehensive test scenarios** with complex queries
- **Data integrity testing** (unique constraints, foreign keys)
- **Performance optimizations** (split queries, retry policies)

## Prerequisites

- **SQL Server LocalDB** (included with Visual Studio)
- **Alternative**: SQL Server Express or full SQL Server instance
- **.NET 8.0 or 9.0**

## Key Files

- `accelergreat.json` - Transaction-based configuration (default)
- `accelergreat.snapshot.json` - Snapshot rollback configuration (alternative)
- `Models/` - E-commerce domain entities and DbContext
- `Components/ECommerceDatabaseComponent.cs` - Advanced database component
- `Tests/` - Comprehensive test suites

## Domain Model

The example uses an e-commerce domain with:
- **Customers** - User accounts with email uniqueness
- **Categories** - Product categorization
- **Products** - Items with pricing and inventory
- **Orders** - Customer purchases with status tracking
- **OrderItems** - Line items with quantities and totals

## Configuration Options

### Transaction-based (Default)
```json
{
  "SqlServerEntityFramework": {
    "ConnectionString": "Server=(localdb)\\MSSQLLocalDB;Database=AccelergreatExample;Trusted_Connection=True;TrustServerCertificate=True",
    "ResetStrategy": "Transactions"
  }
}
```

### Snapshot Rollback (Alternative)
```json
{
  "SqlServerEntityFramework": {
    "ConnectionString": "Server=(localdb)\\MSSQLLocalDB;Database=AccelergreatExample;Trusted_Connection=True;TrustServerCertificate=True",
    "ResetStrategy": "SnapshotRollback",
    "CreateStrategy": "Migrations", 
    "SnapshotDirectory": "C:\\Temp\\AccelergreatSnapshots"
  }
}
```

## Running the Example

```bash
# Navigate to the example directory
cd 02-sqlserver-entity-framework

# Restore packages
dotnet restore

# Run with transaction-based resets (default)
dotnet test

# Run with snapshot rollback (copy alternative config)
copy accelergreat.snapshot.json accelergreat.json
dotnet test
```

## Reset Strategy Comparison

| Strategy | Speed | Complexity | Best For |
|----------|-------|------------|----------|
| **Transactions** | ⚡ 0-3ms | Low | Most scenarios |
| **Snapshot Rollback** | 🐌 100-500ms | Medium | Complex schemas with triggers |

## Advanced Features Demonstrated

### Database Component Configuration
```csharp
protected override void ConfigureDbContextOptions(SqlServerDbContextOptionsBuilder options)
{
    options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    options.EnableRetryOnFailure(maxRetryCount: 3);
}
```

### Comprehensive Data Seeding
- Categories with hierarchical relationships
- Products with inventory tracking
- Customers with unique email constraints
- Orders with complex calculations
- Order items with foreign key relationships

### Complex Query Testing
- Multi-level includes (`Include().ThenInclude()`)
- Aggregate calculations (order totals)
- Date range queries
- Status-based filtering
- Customer order history

## Test Scenarios

### Customer Tests
- ✅ CRUD operations
- ✅ Unique constraint validation
- ✅ Relationship navigation
- ✅ Soft delete patterns

### Order Tests
- ✅ Complex order creation
- ✅ Multi-level entity loading
- ✅ Business logic validation
- ✅ Calculated fields verification
- ✅ Cascade delete handling

## Performance Characteristics

**Transaction Reset Strategy:**
- Database reset: 0-3ms
- Test execution: ~50ms per test
- Parallel execution: Full support

**Snapshot Rollback Strategy:**
- Database reset: 100-500ms
- Test execution: ~200ms per test
- Best for: Complex schemas with triggers

## Connection String Variations

### LocalDB (Recommended for Development)
```
Server=(localdb)\\MSSQLLocalDB;Database=AccelergreatExample;Trusted_Connection=True;TrustServerCertificate=True
```

### SQL Server Express
```
Server=.\\SQLEXPRESS;Database=AccelergreatExample;Trusted_Connection=True;TrustServerCertificate=True
```

### Full SQL Server
```
Server=localhost;Database=AccelergreatExample;Trusted_Connection=True;TrustServerCertificate=True
```

## Troubleshooting

**Q: "Cannot connect to LocalDB"**
A: Install SQL Server LocalDB via Visual Studio installer or SQL Server Express

**Q: "Database already exists" errors**
A: LocalDB persists between runs. Use different database names or clear LocalDB instances

**Q: Slow test execution**
A: Ensure you're using the transaction reset strategy for optimal performance

**Q: Complex relationship loading issues**
A: Check the seeding logic in `OnDatabaseInitializedAsync` for proper ID assignments

## Next Steps

After mastering this example, explore:
- `03-aspnet-api-testing` for web API integration
- `05-microservices-testing` for distributed system testing
- `06-combined-example` for full-stack scenarios

## Production Considerations

- Use connection pooling for better performance
- Implement proper error handling and retries
- Consider using migrations for schema evolution
- Monitor database performance with query splitting
- Use snapshot rollback only when necessary (triggers, complex constraints) 