using System;
using System.Collections.Generic;

namespace SqlServerExample.Models;

public class Customer
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    
    public List<Order> Orders { get; set; } = new();
} 