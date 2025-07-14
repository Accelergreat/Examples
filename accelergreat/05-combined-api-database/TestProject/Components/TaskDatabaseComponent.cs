using System;
using System.Threading.Tasks;
using Accelergreat.EntityFramework.SqlServer;
using Microsoft.Extensions.Configuration;
using TaskApi.Data;
using TaskApi.Models;

namespace CombinedExample.Components;

public class TaskDatabaseComponent : SqlServerEntityFrameworkDatabaseComponent<TaskDbContext>
{
    public TaskDatabaseComponent(IConfiguration configuration) : base(configuration)
    {
    }

    protected override async Task OnDatabaseInitializedAsync(TaskDbContext context)
    {
        // Seed a test project
        var project = new Project
        {
            Name = "Sample Project",
            Description = "A sample project for testing",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System"
        };

        context.Projects.Add(project);
        await context.SaveChangesAsync();

        // Seed test tasks
        var tasks = new[]
        {
            new TaskItem
            {
                Title = "Setup project",
                Description = "Initialize the project structure",
                Priority = TaskPriority.High,
                AssignedTo = "Developer",
                ProjectId = project.Id,
                CreatedAt = DateTime.UtcNow
            },
            new TaskItem
            {
                Title = "Implement API endpoints",
                Description = "Create CRUD endpoints for tasks",
                Priority = TaskPriority.Medium,
                AssignedTo = "Developer",
                ProjectId = project.Id,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Tasks.AddRange(tasks);
        await context.SaveChangesAsync();

        await base.OnDatabaseInitializedAsync(context);
    }
} 