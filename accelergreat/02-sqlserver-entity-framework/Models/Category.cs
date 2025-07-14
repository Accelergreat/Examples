using System;
using System.Collections.Generic;

namespace SqlServerExample.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    
    public List<Product> Products { get; set; } = new();
} 