using System;

namespace BasicSqliteExample.Models;

public class Post
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int BlogId { get; set; }
    public Blog Blog { get; set; } = null!;
} 