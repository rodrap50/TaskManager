using System.ComponentModel;
using Microsoft.AspNetCore.Http;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using TaskManager.Mcp.Data;
using TaskManager.Mcp.Entities;
using TaskManager.Mcp.GrpcClients;

namespace TaskManager.Mcp.Tools;

[McpServerToolType]
public static class AgentStepTools
{
    [McpServerTool(Name = "add_agent_step")]
    [Description("Appends a new step to an existing agent plan, numbered after any existing steps. Requires a non-read-only API token.")]
    public static async Task<McpAgentStep> AddAgentStep(
        McpDbContext db, AuthGrpcClient auth, IHttpContextAccessor httpContextAccessor,
        [Description("The plan's id (GUID) to add this step to.")] string planId,
        [Description("Identifier for the agent performing this step.")] string agentId)
    {
        await McpToolSupport.RequireToken(auth, httpContextAccessor, write: true);
        var plan = await AgentPlanTools.FindPlan(db, planId);
        var nextStepNumber = plan.Steps.Count == 0 ? 1 : plan.Steps.Max(s => s.StepNumber) + 1;

        var now = DateTime.UtcNow;
        var step = new AgentStep
        {
            Id = Guid.NewGuid(), AgentPlanId = plan.Id, StepNumber = nextStepNumber, AgentId = agentId,
            CreatedAt = now, UpdatedAt = now,
        };
        db.AgentSteps.Add(step);
        await db.SaveChangesAsync();
        return step.ToMcp();
    }

    [McpServerTool(Name = "complete_agent_step")]
    [Description("Marks an agent step as completed. Requires a non-read-only API token.")]
    public static async Task<McpAgentStep> CompleteAgentStep(
        McpDbContext db, AuthGrpcClient auth, IHttpContextAccessor httpContextAccessor,
        [Description("The step's id (GUID).")] string id)
    {
        await McpToolSupport.RequireToken(auth, httpContextAccessor, write: true);
        var step = await FindStep(db, id);
        step.Status = AgentStepStatus.Completed;
        step.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return step.ToMcp();
    }

    [McpServerTool(Name = "fail_agent_step")]
    [Description("Marks an agent step as failed. Requires a non-read-only API token.")]
    public static async Task<McpAgentStep> FailAgentStep(
        McpDbContext db, AuthGrpcClient auth, IHttpContextAccessor httpContextAccessor,
        [Description("The step's id (GUID).")] string id)
    {
        await McpToolSupport.RequireToken(auth, httpContextAccessor, write: true);
        var step = await FindStep(db, id);
        step.Status = AgentStepStatus.Failed;
        step.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return step.ToMcp();
    }

    private static async Task<AgentStep> FindStep(McpDbContext db, string id)
    {
        if (!Guid.TryParse(id, out var guid))
            throw new McpException($"'{id}' is not a valid step id.");

        var step = await db.AgentSteps.FindAsync(guid);
        return step ?? throw new McpException($"No agent step found with id '{id}'.");
    }
}
