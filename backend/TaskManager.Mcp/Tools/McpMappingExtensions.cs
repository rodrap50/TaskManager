using TaskManager.Grpc.Contracts;

namespace TaskManager.Mcp.Tools;

public record McpProject(
    string Id, string Name, string? Description, string Scope, string? ColorHex,
    DateTime? DueDate, bool IsArchived, int CriticalityScore, DateTime CreatedAt, DateTime? UpdatedAt);

public record McpEpic(
    string Id, string ProjectId, string Name, string? Description, string? ColorHex,
    int DisplayOrder, DateTime CreatedAt, DateTime? UpdatedAt);

public record McpPhase(
    string Id, string ProjectId, string Name, string? Description,
    int DisplayOrder, DateTime CreatedAt, DateTime? UpdatedAt);

public record McpTask(
    string Id, string ProjectId, string? PhaseId, string? EpicId, string? AssignedUserId,
    string Title, string? Description, string Status, string Priority, int WeightedScore,
    DateTime? DueDate, double? EstimatedHours, bool IsCompleted, DateTime CreatedAt, DateTime? UpdatedAt);

/// <summary>Flattens gRPC contract messages into plain, JSON-friendly shapes for MCP tool output —
/// proto3's optional-scalar Has* tracking and Timestamp structs aren't pleasant for an LLM to read.</summary>
internal static class McpMappingExtensions
{
    public static McpProject ToMcp(this ProjectMessage p) => new(
        p.Id, p.Name, p.HasDescription ? p.Description : null, p.Scope.ToString(),
        p.HasColorHex ? p.ColorHex : null, p.DueDate?.ToDateTime(), p.IsArchived,
        p.CriticalityScore, p.CreatedAt.ToDateTime(), p.UpdatedAt?.ToDateTime());

    public static McpEpic ToMcp(this EpicMessage e) => new(
        e.Id, e.ProjectId, e.Name, e.HasDescription ? e.Description : null,
        e.HasColorHex ? e.ColorHex : null, e.DisplayOrder, e.CreatedAt.ToDateTime(), e.UpdatedAt?.ToDateTime());

    public static McpPhase ToMcp(this ProjectPhaseMessage p) => new(
        p.Id, p.ProjectId, p.Name, p.HasDescription ? p.Description : null,
        p.DisplayOrder, p.CreatedAt.ToDateTime(), p.UpdatedAt?.ToDateTime());

    public static McpTask ToMcp(this ProjectTaskMessage t) => new(
        t.Id, t.ProjectId, t.HasPhaseId ? t.PhaseId : null, t.HasEpicId ? t.EpicId : null,
        t.HasAssignedUserId ? t.AssignedUserId : null, t.Title, t.HasDescription ? t.Description : null,
        t.Status.ToString(), t.Priority.ToString(), t.WeightedScore, t.DueDate?.ToDateTime(),
        t.HasEstimatedHours ? t.EstimatedHours : null, t.IsCompleted, t.CreatedAt.ToDateTime(), t.UpdatedAt?.ToDateTime());
}
