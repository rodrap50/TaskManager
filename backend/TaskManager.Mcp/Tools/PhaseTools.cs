using System.ComponentModel;
using Microsoft.AspNetCore.Http;
using ModelContextProtocol.Server;
using TaskManager.Mcp.GrpcClients;

namespace TaskManager.Mcp.Tools;

[McpServerToolType]
public static class PhaseTools
{
    [McpServerTool(Name = "list_phases", ReadOnly = true)]
    [Description("Lists all phases for a project.")]
    public static async Task<IReadOnlyList<McpPhase>> ListPhases(
        PhasesGrpcClient client, IHttpContextAccessor httpContextAccessor,
        [Description("The parent project's id (GUID).")] string projectId)
    {
        var token = McpToolSupport.GetToken(httpContextAccessor);
        var response = await McpToolSupport.Execute(() => client.ListPhasesByProject(token, projectId));
        return response.Phases.Select(p => p.ToMcp()).ToList();
    }
}
