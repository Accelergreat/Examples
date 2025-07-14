using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskApi.Data;
using TaskApi.Models;
using TaskApi.Models.DTOs;

namespace TaskApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly TaskDbContext _context;
    private readonly ILogger<TasksController> _logger;

    public TasksController(TaskDbContext context, ILogger<TasksController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasks()
    {
        var tasks = await _context.Tasks
            .Include(t => t.Project)
            .Include(t => t.Comments)
            .ToListAsync();

        return Ok(tasks);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskItem>> GetTask(int id)
    {
        var task = await _context.Tasks
            .Include(t => t.Project)
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
            return NotFound();

        return task;
    }

    [HttpPost]
    public async Task<ActionResult<TaskItem>> CreateTask(CreateTaskRequest request)
    {
        _logger.LogInformation("=== CREATE TASK REQUEST RECEIVED ===");
        _logger.LogInformation($"Request: Title='{request?.Title}', Description='{request?.Description}', ProjectId={request?.ProjectId}, AssignedTo='{request?.AssignedTo}'");

        // Add validation error logging
        if (!ModelState.IsValid)
        {
            _logger.LogError("=== CREATE TASK VALIDATION ERRORS ===");
            foreach (var error in ModelState)
            {
                _logger.LogError($"Key: {error.Key}");
                foreach (var err in error.Value.Errors)
                {
                    _logger.LogError($"  Error: {err.ErrorMessage}");
                }
            }
            _logger.LogError("=== END VALIDATION ERRORS ===");
            return BadRequest(ModelState);
        }

        var task = new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            AssignedTo = request.AssignedTo,
            ProjectId = request.ProjectId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        // Load the task with navigation properties for response
        await _context.Entry(task)
            .Reference(t => t.Project)
            .LoadAsync();

        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(int id, CreateTaskRequest request)
    {
        // Add validation error logging
        if (!ModelState.IsValid)
        {
            _logger.LogError("=== UPDATE TASK VALIDATION ERRORS ===");
            foreach (var error in ModelState)
            {
                _logger.LogError($"Key: {error.Key}");
                foreach (var err in error.Value.Errors)
                {
                    _logger.LogError($"  Error: {err.ErrorMessage}");
                }
            }
            _logger.LogError("=== END VALIDATION ERRORS ===");
            return BadRequest(ModelState);
        }

        var existingTask = await _context.Tasks.FindAsync(id);
        if (existingTask == null)
            return NotFound();

        existingTask.Title = request.Title;
        existingTask.Description = request.Description;
        existingTask.Priority = request.Priority;
        existingTask.AssignedTo = request.AssignedTo;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null)
            return NotFound();

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/comments")]
    public async Task<ActionResult<TaskComment>> AddComment(int id, CreateCommentRequest request)
    {
        _logger.LogInformation("=== ADD COMMENT REQUEST RECEIVED ===");
        _logger.LogInformation($"TaskId: {id}, Request: Content='{request?.Content}', CreatedBy='{request?.CreatedBy}'");

        // Add validation error logging
        if (!ModelState.IsValid)
        {
            _logger.LogError("=== ADD COMMENT VALIDATION ERRORS ===");
            foreach (var error in ModelState)
            {
                _logger.LogError($"Key: {error.Key}");
                foreach (var err in error.Value.Errors)
                {
                    _logger.LogError($"  Error: {err.ErrorMessage}");
                }
            }
            _logger.LogError("=== END VALIDATION ERRORS ===");
            return BadRequest(ModelState);
        }

        var task = await _context.Tasks.FindAsync(id);
        if (task == null)
            return NotFound();

        var comment = new TaskComment
        {
            Content = request.Content,
            CreatedBy = request.CreatedBy,
            TaskId = id,
            CreatedAt = DateTime.UtcNow
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, comment);
    }
} 