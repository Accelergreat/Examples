using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TaskApi.Models;

public class TaskComment
{
    public int Id { get; set; }
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    
    public string CreatedBy { get; set; } = string.Empty;
    
    public int TaskId { get; set; }
    
    [JsonIgnore]
    public TaskItem Task { get; set; } = null!;
} 