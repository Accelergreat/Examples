using Microsoft.EntityFrameworkCore;
using TaskApi.Models;

namespace TaskApi.Data;

public class TaskDbContext : DbContext
{
    public TaskDbContext(DbContextOptions<TaskDbContext> options) : base(options)
    {
    }

    public DbSet<TaskItem> Tasks { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<TaskComment> Comments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure TaskItem
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.AssignedTo).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasOne(e => e.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Project
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // Configure TaskComment
        modelBuilder.Entity<TaskComment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasOne(e => e.Task)
                .WithMany(t => t.Comments)
                .HasForeignKey(e => e.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed initial data
        modelBuilder.Entity<Project>().HasData(
            new Project 
            { 
                Id = 1, 
                Name = "Sample Project", 
                Description = "A sample project for testing", 
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            }
        );

        modelBuilder.Entity<TaskItem>().HasData(
            new TaskItem 
            { 
                Id = 1,
                Title = "Setup project",
                Description = "Initial project setup and configuration",
                IsCompleted = true,
                CreatedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow,
                Priority = TaskPriority.High,
                AssignedTo = "Alice Johnson",
                ProjectId = 1
            },
            new TaskItem 
            { 
                Id = 2,
                Title = "Implement API endpoints",
                Description = "Create REST API endpoints for task management",
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow,
                Priority = TaskPriority.Medium,
                AssignedTo = "Bob Smith",
                ProjectId = 1
            }
        );
    }
} 