using System;
using System.Collections.Generic;

namespace BasicSqliteExample.Models;

public class Blog
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<Post> Posts { get; set; } = new();
} 