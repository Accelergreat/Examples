using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Accelergreat.EntityFramework.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using SqlServerExample.Models;

namespace SqlServerExample.Components;

public class ECommerceDatabaseComponent : SqlServerEntityFrameworkDatabaseComponent<ECommerceContext>
{
    public ECommerceDatabaseComponent(IConfiguration configuration) : base(configuration)
    {
    }

    protected override void ConfigureDbContextOptions(SqlServerDbContextOptionsBuilder options)
    {
        options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        // Note: EnableRetryOnFailure is not compatible with Accelergreat's transaction-based reset strategy
    }

    protected override async Task OnDatabaseInitializedAsync(ECommerceContext context)
    {
        // Seed Categories
        var categories = new List<Category>
        {
            new Category { Name = "Electronics", Description = "Electronic devices and gadgets", CreatedAt = DateTime.UtcNow },
            new Category { Name = "Books", Description = "Books and literature", CreatedAt = DateTime.UtcNow },
            new Category { Name = "Clothing", Description = "Apparel and accessories", CreatedAt = DateTime.UtcNow }
        };

        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();

        // Seed Products
        var products = new List<Product>
        {
            new Product 
            { 
                Name = "Laptop", 
                Description = "High-performance laptop", 
                Price = 999.99m, 
                StockQuantity = 50,
                CategoryId = categories[0].Id,
                CreatedAt = DateTime.UtcNow
            },
            new Product 
            { 
                Name = "Smartphone", 
                Description = "Latest smartphone", 
                Price = 699.99m, 
                StockQuantity = 100,
                CategoryId = categories[0].Id,
                CreatedAt = DateTime.UtcNow
            },
            new Product 
            { 
                Name = "Programming Book", 
                Description = "Learn programming fundamentals", 
                Price = 49.99m, 
                StockQuantity = 200,
                CategoryId = categories[1].Id,
                CreatedAt = DateTime.UtcNow
            },
            new Product 
            { 
                Name = "T-Shirt", 
                Description = "Comfortable cotton t-shirt", 
                Price = 19.99m, 
                StockQuantity = 150,
                CategoryId = categories[2].Id,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        // Seed Customers
        var customers = new List<Customer>
        {
            new Customer 
            { 
                Email = "john.doe@example.com", 
                FirstName = "John", 
                LastName = "Doe", 
                CreatedAt = DateTime.UtcNow
            },
            new Customer 
            { 
                Email = "jane.smith@example.com", 
                FirstName = "Jane", 
                LastName = "Smith", 
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Customers.AddRange(customers);
        await context.SaveChangesAsync();

        // Seed Orders
        var order1 = new Order
        {
            CustomerId = customers[0].Id,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Processing,
            Total = 0 // Will be calculated
        };

        var order2 = new Order
        {
            CustomerId = customers[1].Id,
            OrderDate = DateTime.UtcNow.AddDays(-1),
            Status = OrderStatus.Delivered,
            Total = 0 // Will be calculated
        };

        context.Orders.AddRange(order1, order2);
        await context.SaveChangesAsync();

        // Seed Order Items
        var orderItems = new List<OrderItem>
        {
            new OrderItem 
            { 
                OrderId = order1.Id, 
                ProductId = products[0].Id, 
                Quantity = 1, 
                UnitPrice = products[0].Price, 
                TotalPrice = products[0].Price 
            },
            new OrderItem 
            { 
                OrderId = order1.Id, 
                ProductId = products[2].Id, 
                Quantity = 2, 
                UnitPrice = products[2].Price, 
                TotalPrice = products[2].Price * 2 
            },
            new OrderItem 
            { 
                OrderId = order2.Id, 
                ProductId = products[1].Id, 
                Quantity = 1, 
                UnitPrice = products[1].Price, 
                TotalPrice = products[1].Price 
            }
        };

        context.OrderItems.AddRange(orderItems);
        await context.SaveChangesAsync();

        // Update order totals
        order1.Total = orderItems.Where(oi => oi.OrderId == order1.Id).Sum(oi => oi.TotalPrice);
        order2.Total = orderItems.Where(oi => oi.OrderId == order2.Id).Sum(oi => oi.TotalPrice);

        await context.SaveChangesAsync();

        await base.OnDatabaseInitializedAsync(context);
    }
} 