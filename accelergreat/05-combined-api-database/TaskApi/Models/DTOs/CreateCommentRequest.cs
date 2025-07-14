using System.ComponentModel.DataAnnotations;

namespace TaskApi.Models.DTOs;

public class CreateCommentRequest
{
    [Required]
    public string Content { get; set; } = string.Empty;
    
    public string CreatedBy { get; set; } = string.Empty;
} 