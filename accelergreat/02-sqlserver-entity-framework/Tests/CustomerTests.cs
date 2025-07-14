using System;
using System.Linq;
using System.Threading.Tasks;
using Accelergreat.Environments.Pooling;
using Accelergreat.Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SqlServerExample.Components;
using SqlServerExample.Models;
using Xunit;

namespace SqlServerExample.Tests;

public class CustomerTests : AccelergreatXunitTest
{
    public CustomerTests(IAccelergreatEnvironmentPool environmentPool) : base(environmentPool)
    {
    }

    [Fact]
    public async Task CanCreateCustomer()
    {
        // Arrange
        var databaseComponent = GetComponent<ECommerceDatabaseComponent>();
        var dbContextFactory = databaseComponent.DbContextFactory;

        var newCustomer = new Customer
        {
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await using (var context = dbContextFactory.NewDbContext())
        {
            context.Customers.Add(newCustomer);
            await context.SaveChangesAsync();
        }

        // Assert
        await using (var context = dbContextFactory.NewDbContext())
        {
            var savedCustomer = await context.Customers
                .FirstOrDefaultAsync(c => c.Email == "test@example.com");
            
            savedCustomer.Should().NotBeNull();
            savedCustomer!.FirstName.Should().Be("Test");
            savedCustomer.LastName.Should().Be("User");
            savedCustomer.IsActive.Should().BeTrue();
        }
    }

    [Fact]
    public async Task CanRetrieveSeededCustomers()
    {
        // Arrange
        var databaseComponent = GetComponent<ECommerceDatabaseComponent>();
        var dbContextFactory = databaseComponent.DbContextFactory;

        // Act & Assert
        await using var context = dbContextFactory.NewDbContext();
        var customers = await context.Customers.ToListAsync();

        customers.Should().HaveCount(2);
        customers.Should().Contain(c => c.Email == "john.doe@example.com");
        customers.Should().Contain(c => c.Email == "jane.smith@example.com");
    }

    [Fact]
    public async Task CanUpdateCustomer()
    {
        // Arrange
        var databaseComponent = GetComponent<ECommerceDatabaseComponent>();
        var dbContextFactory = databaseComponent.DbContextFactory;

        // Act
        await using (var context = dbContextFactory.NewDbContext())
        {
            var customer = await context.Customers.FirstAsync(c => c.Email == "john.doe@example.com");
            customer.FirstName = "Johnny";
            customer.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }

        // Assert
        await using (var context = dbContextFactory.NewDbContext())
        {
            var updatedCustomer = await context.Customers.FirstAsync(c => c.Email == "john.doe@example.com");
            updatedCustomer.FirstName.Should().Be("Johnny");
            updatedCustomer.UpdatedAt.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task CanDeactivateCustomer()
    {
        // Arrange
        var databaseComponent = GetComponent<ECommerceDatabaseComponent>();
        var dbContextFactory = databaseComponent.DbContextFactory;

        // Act
        await using (var context = dbContextFactory.NewDbContext())
        {
            var customer = await context.Customers.FirstAsync(c => c.Email == "john.doe@example.com");
            customer.IsActive = false;
            customer.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }

        // Assert
        await using (var context = dbContextFactory.NewDbContext())
        {
            var deactivatedCustomer = await context.Customers.FirstAsync(c => c.Email == "john.doe@example.com");
            deactivatedCustomer.IsActive.Should().BeFalse();
        }
    }

    [Fact]
    public async Task CanGetCustomerWithOrders()
    {
        // Arrange
        var databaseComponent = GetComponent<ECommerceDatabaseComponent>();
        var dbContextFactory = databaseComponent.DbContextFactory;

        // Act
        await using var context = dbContextFactory.NewDbContext();
        var customerWithOrders = await context.Customers
            .Include(c => c.Orders)
            .FirstAsync(c => c.Email == "john.doe@example.com");

        // Assert
        customerWithOrders.Orders.Should().HaveCount(1);
        customerWithOrders.Orders.First().Status.Should().Be(OrderStatus.Processing);
    }

    [Fact]
    public async Task EmailUniquenessIsEnforced()
    {
        // Arrange
        var databaseComponent = GetComponent<ECommerceDatabaseComponent>();
        var dbContextFactory = databaseComponent.DbContextFactory;

        // Act & Assert
        await using var context = dbContextFactory.NewDbContext();
        var duplicateCustomer = new Customer
        {
            Email = "john.doe@example.com", // This email already exists
            FirstName = "Another",
            LastName = "John",
            CreatedAt = DateTime.UtcNow
        };

        context.Customers.Add(duplicateCustomer);

        // This should throw due to unique constraint
        var act = async () => await context.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>();
    }
} 