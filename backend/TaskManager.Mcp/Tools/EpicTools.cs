using System.ComponentModel;
using Microsoft.AspNetCore.Http;
using ModelContextProtocol.Server;
using TaskManager.Grpc.Contracts;
using TaskManager.Mcp.GrpcClients;

namespace TaskManager.Mcp.Tools;

[McpServerToolType]
public static class EpicTools
{
    [McpServerTool(Name = "list_epics", ReadOnly = true)]
    [Description("Lists all epics for a project.")]
    public static async Task<IReadOnlyList<McpEpic>> ListEpics(
        EpicsGrpcClient client, IHttpContextAccessor httpContextAccessor,
        [Description("The parent project's id (GUID).")] string projectId)
    {
        var token = McpToolSupport.GetToken(httpContextAccessor);
        var response = await McpToolSupport.Execute(() => client.ListEpicsByProject(token, projectId));
        return response.Epics.Select(e => e.ToMcp()).ToList();
    }

    [McpServerTool(Name = "create_epic")]
    [Description("Creates a new epic under a project. Requires a non-read-only API token.")]
    public static async Task<McpEpic> CreateEpic(
        EpicsGrpcClient client, IHttpContextAccessor httpContextAccessor,
        [Description("The parent project's id (GUID).")] string projectId,
        [Description("Epic name.")] string name,
        [Description("Display order among sibling epics.")] int displayOrder,
        [Description("Optional description.")] string? description = null,
        [Description("Optional hex color, e.g. #FF0000.")] string? colorHex = null)
    {
        var token = McpToolSupport.GetToken(httpContextAccessor);
        var request = new CreateEpicRequest { ProjectId = projectId, Name = name, DisplayOrder = displayOrder };
        if (description is not null) request.Description = description;
        if (colorHex is not null) request.ColorHex = colorHex;

        var epic = await McpToolSupport.Execute(() => client.CreateEpic(token, request));
        return epic.ToMcp();
    }

    [McpServerTool(Name = "update_epic")]
    [Description("Updates an existing epic. Omitted optional fields are left unchanged. Requires a non-read-only API token.")]
    public static async Task<McpEpic> UpdateEpic(
        EpicsGrpcClient client, IHttpContextAccessor httpContextAccessor,
        [Description("The epic's id (GUID).")] string id,
        [Description("New name, or omit to leave unchanged.")] string? name = null,
        [Description("New description, or omit to leave unchanged.")] string? description = null,
        [Description("New display order, or omit to leave unchanged.")] int? displayOrder = null)
    {
        var token = McpToolSupport.GetToken(httpContextAccessor);
        var request = new UpdateEpicRequest { Id = id };
        if (name is not null) request.Name = name;
        if (description is not null) request.Description = description;
        if (displayOrder is not null) request.DisplayOrder = displayOrder.Value;

        var epic = await McpToolSupport.Execute(() => client.UpdateEpic(token, request));
        return epic.ToMcp();
    }
}
