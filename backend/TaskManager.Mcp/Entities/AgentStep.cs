namespace TaskManager.Mcp.Entities;

public enum AgentStepStatus
{
    Pending,
    InProgress,
    Completed,
    Failed,
    Skipped
}

public class AgentStep
{
    public Guid Id { get; set; }
    public required Guid AgentPlanId { get; set; }
    public AgentPlan? AgentPlan { get; set; }
    public required int StepNumber { get; set; }
    public AgentStepStatus Status { get; set; } = AgentStepStatus.Pending;
    public required string AgentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
