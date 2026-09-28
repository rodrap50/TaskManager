using System.ComponentModel;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Http;
using ModelContextProtocol.Server;
using TaskManager.Grpc.Contracts;
using TaskManager.Mcp.GrpcClients;

namespace TaskManager.Mcp.Tools;

[McpServerToolType]
public static class TaskTools
{
    [McpServerTool(Name = "list_tasks", ReadOnly = true)]
    [Description("Lists all tasks for a project.")]
    public static async Task<IReadOnlyList<McpTask>> ListTasks(
        TasksGrpcClient client, IHttpContextAccessor httpContextAccessor,
        [Description("The parent project's id (GUID).")] string projectId)
    {
        var token = McpToolSupport.GetToken(httpContextAccessor);
        var response = await McpToolSupport.Execute(() => client.ListTasksByProject(token, projectId));
        return response.Tasks.Select(t => t.ToMcp()).ToList();
    }

    [McpServerTool(Name = "get_task", ReadOnly = true)]
    [Description("Gets a single task by id.")]
    public static async Task<McpTask> GetTask(
        TasksGrpcClient client, IHttpContextAccessor httpContextAccessor,
        [Description("The task's id (GUID).")] string id)
    {
        var token = McpToolSupport.GetToken(httpContextAccessor);
        var task = await McpToolSupport.Execute(() => client.GetTask(token, id));
        return task.ToMcp();
    }

    [McpServerTool(Name = "create_task")]
    [Description("Creates a new task under a project. Requires a non-read-only API token.")]
    public static async Task<McpTask> CreateTask(
        TasksGrpcClient client, IHttpContextAccessor httpContextAccessor,
        [Description("The parent project's id (GUID).")] string projectId,
        [Description("Task title.")] string title,
        [Description("Priority: Low, Medium, High, or Critical.")] string priority = "Medium",
        [Description("Optional description.")] string? description = null,
        [Description("Optional phase id (GUID) to place the task in.")] string? phaseId = null,
        [Description("Optional epic id (GUID) to associate the task with.")] string? epicId = null,
        [Description("Optional assignee user id (GUID).")] string? assignedUserId = null,
        [Description("Optional due date (UTC).")] DateTime? dueDate = null,
        [Description("Optional estimated effort in hours.")] double? estimatedHours = null)
    {
        var token = McpToolSupport.GetToken(httpContextAccessor);
        var request = new CreateTaskRequest
        {
            ProjectId = projectId, Title = title, Priority = McpEnumParsing.ParsePriority(priority),
        };
        if (description is not null) request.Description = description;
        if (phaseId is not null) request.PhaseId = phaseId;
        if (epicId is not null) request.EpicId = epicId;
        if (assignedUserId is not null) request.AssignedUserId = assignedUserId;
        if (dueDate is { } due) request.DueDate = Timestamp.FromDateTime(DateTime.SpecifyKind(due, DateTimeKind.Utc));
        if (estimatedHours is not null) request.EstimatedHours = estimatedHours.Value;

        var task = await McpToolSupport.Execute(() => client.CreateTask(token, request));
        return task.ToMcp();
    }

    // ponytail: doesn't expose UpdateTaskRequest's clear_* flags (clear phase/epic/assignee
    // without setting a replacement) — every field here is set-if-provided only. Add a
    // clearPhase/clearEpic/clearAssignee bool param each if an agent needs to un-assign rather
    // than re-assign.
    [McpServerTool(Name = "update_task")]
    [Description("Updates an existing task. Omitted optional fields are left unchanged. Requires a non-read-only API token.")]
    public static async Task<McpTask> UpdateTask(
        TasksGrpcClient client, IHttpContextAccessor httpContextAccessor,
        [Description("The task's id (GUID).")] string id,
        [Description("New title, or omit to leave unchanged.")] string? title = null,
        [Description("New description, or omit to leave unchanged.")] string? description = null,
        [Description("New priority (Low, Medium, High, Critical), or omit to leave unchanged.")] string? priority = null,
        [Description("New phase id (GUID), or omit to leave unchanged.")] string? phaseId = null,
        [Description("New epic id (GUID), or omit to leave unchanged.")] string? epicId = null,
        [Description("New assignee user id (GUID), or omit to leave unchanged.")] string? assignedUserId = null,
        [Description("New due date (UTC), or omit to leave unchanged.")] DateTime? dueDate = null,
        [Description("New estimated effort in hours, or omit to leave unchanged.")] double? estimatedHours = null)
    {
        var token = McpToolSupport.GetToken(httpContextAccessor);
        var request = new UpdateTaskRequest { Id = id };
        if (title is not null) request.Title = title;
        if (description is not null) request.Description = description;
        if (priority is not null) request.Priority = McpEnumParsing.ParsePriority(priority);
        if (phaseId is not null) request.PhaseId = phaseId;
        if (epicId is not null) request.EpicId = epicId;
        if (assignedUserId is not null) request.AssignedUserId = assignedUserId;
        if (dueDate is { } due) request.DueDate = Timestamp.FromDateTime(DateTime.SpecifyKind(due, DateTimeKind.Utc));
        if (estimatedHours is not null) request.EstimatedHours = estimatedHours.Value;

        var task = await McpToolSupport.Execute(() => client.UpdateTask(token, request));
        return task.ToMcp();
    }

    [McpServerTool(Name = "transition_task")]
    [Description("Transitions a task to a new workflow status. Requires a non-read-only API token.")]
    public static async Task<McpTask> TransitionTask(
        TasksGrpcClient client, IHttpContextAccessor httpContextAccessor,
        [Description("The task's id (GUID).")] string id,
        [Description("New status: Backlog, InProgress, Blocked, Done, or Cancelled.")] string newStatus)
    {
        var token = McpToolSupport.GetToken(httpContextAccessor);
        var task = await McpToolSupport.Execute(
            () => client.TransitionTask(token, id, McpEnumParsing.ParseStatus(newStatus)));
        return task.ToMcp();
    }
}
