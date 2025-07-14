# ASP.NET Core API Testing Example

This example demonstrates comprehensive API testing using Accelergreat without database dependencies. It shows how to test RESTful endpoints, HTTP methods, JSON serialization, and error scenarios.

## What This Example Shows

- **Pure API Testing** - No database dependencies, in-memory data storage
- **RESTful Endpoint Testing** - GET, POST, PUT, DELETE operations
- **HTTP Status Code Validation** - 200, 201, 400, 404, etc.
- **JSON Request/Response Testing** - Serialization and deserialization
- **Validation Logic Testing** - Required fields, business rules
- **Error Scenario Testing** - Invalid inputs, missing data
- **Parallel Test Execution** - Multiple simultaneous requests
- **Complete CRUD Workflows** - End-to-end API scenarios

## Project Structure

```
03-aspnet-api-testing/
├── TestApi/                          # ASP.NET Core API Project
│   ├── Controllers/                  # API Controllers
│   │   ├── WeatherForecastController.cs
│   │   ├── UsersController.cs
│   │   └── ProductsController.cs
│   ├── Models/                       # Data Models
│   │   ├── WeatherForecast.cs
│   │   ├── User.cs
│   │   └── Product.cs
│   ├── Services/                     # Business Logic
│   │   ├── IUserService.cs
│   │   ├── UserService.cs
│   │   ├── IProductService.cs
│   │   └── ProductService.cs
│   ├── Program.cs                    # API Startup
│   └── TestApi.csproj               # API Project File
├── Components/                       # Accelergreat Components
│   └── TestApiWebAppComponent.cs    # Web App Component
├── Tests/                           # Test Classes
│   ├── WeatherForecastControllerTests.cs
│   ├── UsersControllerTests.cs
│   └── ProductsControllerTests.cs
├── accelergreat.json               # Accelergreat Configuration
├── Startup.cs                      # Test Startup Configuration
├── AspNetApiExample.csproj         # Test Project File
└── README.md                       # This file
```

## API Endpoints

### WeatherForecast Controller
- `GET /api/weatherforecast` - Get 5-day forecast
- `GET /api/weatherforecast/{days}` - Get forecast for specified days
- `GET /api/weatherforecast/current` - Get current weather

### Users Controller  
- `GET /api/users` - Get all users
- `GET /api/users/{id}` - Get user by ID
- `POST /api/users` - Create new user
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user (soft delete)

### Products Controller
- `GET /api/products` - Get all products
- `GET /api/products/{id}` - Get product by ID
- `GET /api/products/search?q={term}` - Search products
- `POST /api/products` - Create new product
- `PUT /api/products/{id}` - Update product
- `DELETE /api/products/{id}` - Delete product

## Key Features Demonstrated

### In-Memory Data Management
- **UserService**: Manages user data with seeded test users
- **ProductService**: Manages product catalog with sample products
- **Singleton Services**: Data persists for the duration of each test

### HTTP Method Testing
```csharp
[Fact]
public async Task GetAllUsers_ShouldReturnSeededUsers()
{
    var response = await client.GetAsync("/api/users");
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}

[Fact]
public async Task CreateUser_WithValidData_ShouldCreateUser()
{
    var response = await client.PostAsync("/api/users", content);
    response.StatusCode.Should().Be(HttpStatusCode.Created);
}
```

### JSON Request/Response Testing
```csharp
var users = JsonConvert.DeserializeObject<User[]>(content);
users.Should().HaveCount(3);
users.Should().Contain(u => u.Name == "John Doe");
```

### Validation Testing
```csharp
[Fact]
public async Task CreateUser_WithMissingName_ShouldReturnBadRequest()
{
    var invalidUser = new User { Email = "test@example.com" };
    // Name is missing - should return BadRequest
}
```

### Error Scenario Testing
```csharp
[Fact]
public async Task GetUserById_WithInvalidId_ShouldReturnNotFound()
{
    var response = await client.GetAsync("/api/users/999");
    response.StatusCode.Should().Be(HttpStatusCode.NotFound);
}
```

## Running the Example

```bash
# Navigate to the example directory
cd 03-aspnet-api-testing

# Restore packages
dotnet restore

# Run the tests
dotnet test

# Run with verbose output
dotnet test --verbosity normal
```

## Test Coverage

### WeatherForecast Tests (6 tests)
- ✅ Default forecast generation
- ✅ Custom day count requests
- ✅ Input validation (invalid day counts)
- ✅ Current weather endpoint
- ✅ Temperature calculation accuracy
- ✅ Parallel request handling

### Users Tests (10 tests)
- ✅ Get all users
- ✅ Get user by ID (valid/invalid)
- ✅ Create user (valid/invalid data)
- ✅ Update user (valid/invalid scenarios)
- ✅ Delete user (soft delete)
- ✅ Complete workflow testing

### Products Tests (12 tests)
- ✅ Get all products
- ✅ Get product by ID
- ✅ Search functionality
- ✅ Create product (validation testing)
- ✅ Update product
- ✅ Delete product
- ✅ Complete lifecycle testing

## Performance Characteristics

- **Test Execution**: ~100-200ms per test
- **Parallel Execution**: Full support
- **Memory Usage**: Minimal (in-memory only)
- **No Database**: No cleanup/reset overhead

## Accelergreat Benefits Demonstrated

### 1. **Isolated Test Environments**
Each test gets its own API instance with fresh in-memory data.

### 2. **Parallel Execution**
Tests run simultaneously without interference.

### 3. **No External Dependencies**
Pure HTTP testing without database or external service dependencies.

### 4. **Realistic API Testing**
Full ASP.NET Core pipeline including routing, model binding, and JSON serialization.

## Common Patterns

### Test Structure
```csharp
public class UsersControllerTests : AccelergreatXunitTest
{
    public UsersControllerTests(IAccelergreatEnvironmentPool environmentPool) 
        : base(environmentPool) { }

    [Fact]
    public async Task TestMethod()
    {
        // Arrange
        var webAppComponent = GetComponent<TestApiWebAppComponent>();
        var client = webAppComponent.CreateClient();

        // Act
        var response = await client.GetAsync("/api/endpoint");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

### JSON Testing
```csharp
var content = await response.Content.ReadAsStringAsync();
var result = JsonConvert.DeserializeObject<Model>(content);
result.Should().NotBeNull();
result!.Property.Should().Be(expectedValue);
```

### Workflow Testing
```csharp
// Create -> Read -> Update -> Delete workflow
var createResponse = await client.PostAsync(url, createContent);
var createdItem = JsonConvert.DeserializeObject<Model>(await createResponse.Content.ReadAsStringAsync());

var getResponse = await client.GetAsync($"{url}/{createdItem.Id}");
// ... continue workflow
```

## Best Practices Demonstrated

1. **Arrange-Act-Assert Pattern** - Clear test structure
2. **Descriptive Test Names** - Intent is obvious from name
3. **Comprehensive Assertions** - Status codes, content, headers
4. **Edge Case Testing** - Invalid inputs, boundary conditions
5. **Workflow Testing** - End-to-end scenarios
6. **Parallel Safety** - Tests don't interfere with each other

## Next Steps

After mastering this example, explore:
- `05-microservices-testing` for service-to-service communication
- `06-combined-example` for API + database integration
- `02-sqlserver-entity-framework` for database-driven APIs

## Troubleshooting

**Q: Tests fail with "Connection refused"**
A: Check that the TestApi project builds correctly and all dependencies are restored

**Q: JSON deserialization errors**
A: Verify model properties match API response structure

**Q: Random test failures**
A: Each test gets isolated data - check for hardcoded assumptions about data state

**Q: Slow test execution**
A: API tests should be fast (~100ms) - check for unnecessary delays or timeouts 