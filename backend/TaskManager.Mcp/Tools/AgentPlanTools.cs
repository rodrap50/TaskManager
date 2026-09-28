using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using TaskManager.Mcp.Data;
using TaskManager.Mcp.Entities;

namespace TaskManager.Mcp.Tools;

public record McpAgentPlan(
    Guid Id, Guid ProjectId, string Status, string AgentId, string SessionId,
    DateTime CreatedAt, DateTime UpdatedAt, IReadOnlyList<McpAgentStep> Steps);

public record McpAgentStep(Guid Id, int StepNumber, string Status, string AgentId, DateTime CreatedAt, DateTime UpdatedAt);

[McpServerToolType]
public static class AgentPlanTools
{
    [McpServerTool(Name = "create_agent_plan")]
    [Description("Creates a new agent plan for tracking multi-step work against a project. Lives entirely in TaskManager.Mcp's own tracking store — no call to TaskManager.API.")]
    public static async Task<McpAgentPlan> CreateAgentPlan(
        McpDbContext db,
        [Description("The project id (GUID) this plan is working against.")] string projectId,
        [Description("Identifier for the calling agent, e.g. \"claude-desktop\".")] string agentId,
        [Description("Identifier for the current agent session.")] string sessionId)
    {
        if (!Guid.TryParse(projectId, out var projectGuid))
            throw new McpException($"'{projectId}' is not a valid project id.");

        var now = DateTime.UtcNow;
        var plan = new AgentPlan
        {
            Id = Guid.NewGuid(), ProjectId = projectGuid, AgentId = agentId, SessionId = sessionId,
            CreatedAt = now, UpdatedAt = now,
        };
        db.AgentPlans.Add(plan);
        await db.SaveChangesAsync();
        return plan.ToMcp();
    }

    [McpServerTool(Name = "get_agent_plan", ReadOnly = true)]
    [Description("Gets an agent plan's current status and its steps.")]
    public static async Task<McpAgentPlan> GetAgentPlan(
        McpDbContext db, [Description("The plan's id (GUID).")] string id)
    {
        var plan = await FindPlan(db, id);
        return plan.ToMcp();
    }

    [McpServerTool(Name = "complete_agent_plan")]
    [Description("Marks an agent plan as completed.")]
    public static async Task<McpAgentPlan> CompleteAgentPlan(
        McpDbContext db, [Description("The plan's id (GUID).")] string id)
    {
        var plan = await FindPlan(db, id);
        plan.Status = AgentPlanStatus.Completed;
        plan.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return plan.ToMcp();
    }

    internal static async Task<AgentPlan> FindPlan(McpDbContext db, string id)
    {
        if (!Guid.TryParse(id, out var guid))
            throw new McpException($"'{id}' is not a valid plan id.");

        var plan = await db.AgentPlans.Include(p => p.Steps).FirstOrDefaultAsync(p => p.Id == guid);
        return plan ?? throw new McpException($"No agent plan found with id '{id}'.");
    }
}

internal static class AgentPlanMappingExtensions
{
    public static McpAgentPlan ToMcp(this AgentPlan p) => new(
        p.Id, p.ProjectId, p.Status.ToString(), p.AgentId, p.SessionId, p.CreatedAt, p.UpdatedAt,
        p.Steps.OrderBy(s => s.StepNumber).Select(s => s.ToMcp()).ToList());

    public static McpAgentStep ToMcp(this AgentStep s) =>
        new(s.Id, s.StepNumber, s.Status.ToString(), s.AgentId, s.CreatedAt, s.UpdatedAt);
}
