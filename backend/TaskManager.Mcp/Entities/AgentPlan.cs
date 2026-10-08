namespace TaskManager.Mcp.Entities;

public enum AgentPlanStatus
{
    Pending,
    InProgress,
    Completed,
    Failed
}

/// <summary>
/// Tracks a unit of work an MCP-connected agent is carrying out against a project.
/// Lives only in the standalone McpTracking database — no FK to the monolith's schema.
/// </summary>
public class AgentPlan
{
    public Guid Id { get; set; }
    public required Guid ProjectId { get; set; }
    public AgentPlanStatus Status { get; set; } = AgentPlanStatus.Pending;
    public required string AgentId { get; set; }
    public required string SessionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<AgentStep> Steps { get; set; } = [];
}
