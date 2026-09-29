using ModelContextProtocol;
using TaskManager.Grpc.Contracts;

namespace TaskManager.Mcp.Tools;

/// <summary>Accepts plain words ("Low", "InProgress") from MCP tool callers instead of the
/// shouty proto enum names (TASK_PRIORITY_LOW) — friendlier for an LLM to produce correctly.</summary>
internal static class McpEnumParsing
{
    public static TaskPriorityValue ParsePriority(string value) => value.Trim().ToLowerInvariant() switch
    {
        "low" => TaskPriorityValue.TaskPriorityLow,
        "medium" => TaskPriorityValue.TaskPriorityMedium,
        "high" => TaskPriorityValue.TaskPriorityHigh,
        "critical" => TaskPriorityValue.TaskPriorityCritical,
        _ => throw new McpException($"Invalid priority '{value}'. Expected one of: Low, Medium, High, Critical."),
    };

    public static TaskStatusValue ParseStatus(string value) => value.Trim().ToLowerInvariant() switch
    {
        "backlog" => TaskStatusValue.TaskStatusBacklog,
        "inprogress" or "in_progress" or "in-progress" => TaskStatusValue.TaskStatusInProgress,
        "blocked" => TaskStatusValue.TaskStatusBlocked,
        "done" => TaskStatusValue.TaskStatusDone,
        "cancelled" or "canceled" => TaskStatusValue.TaskStatusCancelled,
        _ => throw new McpException($"Invalid status '{value}'. Expected one of: Backlog, InProgress, Blocked, Done, Cancelled."),
    };
}
