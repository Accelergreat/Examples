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

public class OrderTests : AccelergreatXunitTest
{
    public OrderTests(IAccelergreatEnvironmentPool environmentPool) : base(environmentPool)
    {
    }

    [Fact]
    public async Task CanCreateOrderWithItems()
    {
        // Arrange
        var databaseComponent = GetComponent<ECommerceDatabaseComponent>();
        var dbContextFactory = databaseComponent.DbContextFactory;

        Customer customer;
        Product product;
        
        await using (var context = dbContextFactory.NewDbContext())
        {
            customer = await context.Customers.FirstAsync();
            product = await context.Products.FirstAsync();
        }

        var newOrder = new Order
        {
            CustomerId = customer.Id,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            Total = 0
        };

        // Act
        await using (var context = dbContextFactory.NewDbContext())
        {
            context.Orders.Add(newOrder);
            await context.SaveChangesAsync();

            var orderItem = new OrderItem
            {
                OrderId = newOrder.Id,
                ProductId = product.Id,
                Quantity = 2,
                UnitPrice = product.Price,
                TotalPrice = product.Price * 2
            };

            context.OrderItems.Add(orderItem);
            await context.SaveChangesAsync();

            // Update order total
            newOrder.Total = orderItem.TotalPrice;
            await context.SaveChangesAsync();
        }

        // Assert
        await using (var context = dbContextFactory.NewDbContext())
        {
            var savedOrder = await context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstAsync(o => o.Id == newOrder.Id);

            savedOrder.OrderItems.Should().HaveCount(1);
            savedOrder.OrderItems.First().Quantity.Should().Be(2);
            savedOrder.OrderItems.First().Product.Name.Should().Be(product.Name);
            savedOrder.Total.Should().Be(product.Price * 2);
        }
    }

    [Fact]
    public async Task CanRetrieveOrdersWithFullDetails()
    {
        // Arrange
        var databaseComponent = GetComponent<ECommerceDatabaseComponent>();
        var dbContextFactory = databaseComponent.DbContextFactory;

        // Act
        await using var context = dbContextFactory.NewDbContext();
        var orders = await context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .ThenInclude(p => p.Category)
            .ToListAsync();

        // Assert
        orders.Should().HaveCount(2);
        
        var processingOrder = orders.First(o => o.Status == OrderStatus.Processing);
        processingOrder.Customer.Should().NotBeNull();
        processingOrder.Customer.Email.Should().Be("john.doe@example.com");
        processingOrder.OrderItems.Should().HaveCount(2);
        
        var deliveredOrder = orders.First(o => o.Status == OrderStatus.Delivered);
        deliveredOrder.Customer.Email.Should().Be("jane.smith@example.com");
        deliveredOrder.OrderItems.Should().HaveCount(1);
    }

    [Fact]
    public async Task CanUpdateOrderStatus()
    {
        // Arrange
        var databaseComponent = GetComponent<ECommerceDatabaseComponent>();
        var dbContextFactory = databaseComponent.DbContextFactory;

        // Act
        await using (var context = dbContextFactory.NewDbContext())
        {
            var order = await context.Orders.FirstAsync(o => o.Status == OrderStatus.Processing);
            order.Status = OrderStatus.Shipped;
            order.Notes = "Shipped via express delivery";
            await context.SaveChangesAsync();
        }

        // Assert
        await using (var context = dbContextFactory.NewDbContext())
        {
            var updatedOrder = await context.Orders.FirstAsync(o => o.Notes == "Shipped via express delivery");
            updatedOrder.Status.Should().Be(OrderStatus.Shipped);
        }
    }

    [Fact]
    public async Task CanCalculateOrderTotals()
    {
        // Arrange
        var databaseComponent = GetComponent<ECommerceDatabaseComponent>();
        var dbContextFactory = databaseComponent.DbContextFactory;

        // Act
        await using var context = dbContextFactory.NewDbContext();
        var ordersWithTotals = await context.Orders
            .Include(o => o.OrderItems)
            .Select(o => new
            {
                OrderId = o.Id,
                StoredTotal = o.Total,
                CalculatedTotal = o.OrderItems.Sum(oi => oi.TotalPrice)
            })
            .ToListAsync();

        // Assert
        foreach (var order in ordersWithTotals)
        {
            order.StoredTotal.Should().Be(order.CalculatedTotal);
        }
    }

    [Fact]
    public async Task CanGetOrdersByCustomer()
    {
        // Arrange
        var databaseComponent = GetComponent<ECommerceDatabaseComponent>();
        var dbContextFactory = databaseComponent.DbContextFactory;

        // Act
        await using var context = dbContextFactory.NewDbContext();
        var johnOrders = await context.Orders
            .Include(o => o.Customer)
            .Where(o => o.Customer.Email == "john.doe@example.com")
            .ToListAsync();

        // Assert
        johnOrders.Should().HaveCount(1);
        johnOrders.First().Status.Should().Be(OrderStatus.Processing);
    }

    [Fact]
    public async Task CanGetOrdersByStatus()
    {
        // Arrange
        var databaseComponent = GetComponent<ECommerceDatabaseComponent>();
        var dbContextFactory = databaseComponent.DbContextFactory;

        // Act
        await using var context = dbContextFactory.NewDbContext();
        var pendingOrders = await context.Orders
            .Where(o => o.Status == OrderStatus.Pending)
            .ToListAsync();

        var deliveredOrders = await context.Orders
            .Where(o => o.Status == OrderStatus.Delivered)
            .ToListAsync();

        // Assert
        pendingOrders.Should().BeEmpty();
        deliveredOrders.Should().HaveCount(1);
    }

    [Fact]
    public async Task CanGetOrdersByDateRange()
    {
        // Arrange
        var databaseComponent = GetComponent<ECommerceDatabaseComponent>();
        var dbContextFactory = databaseComponent.DbContextFactory;

        var today = DateTime.UtcNow.Date;
        var yesterday = today.AddDays(-1);

        // Act
        await using var context = dbContextFactory.NewDbContext();
        var todayOrders = await context.Orders
            .Where(o => o.OrderDate.Date == today)
            .ToListAsync();

        var yesterdayOrders = await context.Orders
            .Where(o => o.OrderDate.Date == yesterday)
            .ToListAsync();

        // Assert
        todayOrders.Should().HaveCount(1);
        yesterdayOrders.Should().HaveCount(1);
    }

    [Fact]
    public async Task CanDeleteOrderWithItems()
    {
        // Arrange
        var databaseComponent = GetComponent<ECommerceDatabaseComponent>();
        var dbContextFactory = databaseComponent.DbContextFactory;

        int orderToDeleteId;
        await using (var context = dbContextFactory.NewDbContext())
        {
            var orderToDelete = await context.Orders
                .Include(o => o.OrderItems)
                .FirstAsync(o => o.Status == OrderStatus.Delivered);
            orderToDeleteId = orderToDelete.Id;
        }

        // Act
        await using (var context = dbContextFactory.NewDbContext())
        {
            // Delete order items first due to foreign key constraint
            var items = await context.OrderItems
                .Where(oi => oi.OrderId == orderToDeleteId)
                .ToListAsync();
            
            context.OrderItems.RemoveRange(items);
            await context.SaveChangesAsync();

            // Then delete the order - fetch it in this context to avoid concurrency issues
            var orderToDelete = await context.Orders.FirstAsync(o => o.Id == orderToDeleteId);
            context.Orders.Remove(orderToDelete);
            await context.SaveChangesAsync();
        }

        // Assert
        await using (var context = dbContextFactory.NewDbContext())
        {
            var deletedOrder = await context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderToDeleteId);
            
            deletedOrder.Should().BeNull();

            var deletedItems = await context.OrderItems
                .Where(oi => oi.OrderId == orderToDeleteId)
                .ToListAsync();
            
            deletedItems.Should().BeEmpty();
        }
    }
} 