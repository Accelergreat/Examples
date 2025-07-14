using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TaskApi.Models;

public class Project
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    
    public string CreatedBy { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
    
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
} 