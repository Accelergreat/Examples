using System;
using System.Collections.Generic;

namespace SqlServerExample.Models;

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal Total { get; set; }
    public OrderStatus Status { get; set; }
    public string? Notes { get; set; }
    
    public Customer Customer { get; set; } = null!;
    public List<OrderItem> OrderItems { get; set; } = new();
}

public enum OrderStatus
{
    Pending = 0,
    Processing = 1,
    Shipped = 2,
    Delivered = 3,
    Cancelled = 4
} 