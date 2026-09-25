using System.ComponentModel;
using Microsoft.AspNetCore.Http;
using ModelContextProtocol.Server;
using TaskManager.Mcp.GrpcClients;

namespace TaskManager.Mcp.Tools;

[McpServerToolType]
public static class ProjectTools
{
    [McpServerTool(Name = "list_projects", ReadOnly = true)]
    [Description("Lists all projects visible to the caller.")]
    public static async Task<IReadOnlyList<McpProject>> ListProjects(
        ProjectsGrpcClient client, IHttpContextAccessor httpContextAccessor)
    {
        var token = McpToolSupport.GetToken(httpContextAccessor);
        var response = await McpToolSupport.Execute(() => client.ListProjects(token));
        return response.Projects.Select(p => p.ToMcp()).ToList();
    }

    [McpServerTool(Name = "get_project", ReadOnly = true)]
    [Description("Gets a single project by id.")]
    public static async Task<McpProject> GetProject(
        ProjectsGrpcClient client, IHttpContextAccessor httpContextAccessor,
        [Description("The project's id (GUID).")] string id)
    {
        var token = McpToolSupport.GetToken(httpContextAccessor);
        var project = await McpToolSupport.Execute(() => client.GetProject(token, id));
        return project.ToMcp();
    }
}
