using Microsoft.EntityFrameworkCore;
using TaskManager.Mcp.Entities;

namespace TaskManager.Mcp.Data;

/// <summary>
/// Standalone EF Core context for TaskManager.Mcp's own McpTracking database.
/// Deliberately has no reference to TaskManager.Domain or TaskManager.Infrastructure —
/// agent-tracking data is not FK-coupled to the monolith's schema.
/// </summary>
public class McpDbContext(DbContextOptions<McpDbContext> options) : DbContext(options)
{
    public DbSet<AgentPlan> AgentPlans => Set<AgentPlan>();
    public DbSet<AgentStep> AgentSteps => Set<AgentStep>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AgentPlan>()
            .HasMany(p => p.Steps)
            .WithOne(s => s.AgentPlan)
            .HasForeignKey(s => s.AgentPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AgentStep>()
            .HasIndex(s => new { s.AgentPlanId, s.StepNumber })
            .IsUnique();
    }
}
