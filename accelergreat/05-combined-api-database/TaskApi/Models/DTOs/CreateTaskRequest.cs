using System.ComponentModel.DataAnnotations;

namespace TaskApi.Models.DTOs;

public class CreateTaskRequest
{
    [Required]
    public string Title { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    
    public string AssignedTo { get; set; } = string.Empty;
    
    public int ProjectId { get; set; }
} 