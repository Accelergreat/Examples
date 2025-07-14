using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TaskApi.Models;

public class TaskItem
{
    public int Id { get; set; }
    
    [Required]
    public string Title { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public bool IsCompleted { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? CompletedAt { get; set; }
    
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    
    public string AssignedTo { get; set; } = string.Empty;
    
    public int ProjectId { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Project Project { get; set; } = null!;
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
}

public enum TaskPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
} 