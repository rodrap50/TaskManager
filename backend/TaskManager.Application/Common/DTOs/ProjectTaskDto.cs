namespace TaskManager.Application.Common.DTOs;

/// <summary>
/// <paramref name="Votes"/> maps every voter's <c>UserId</c> to their cast <c>VoteValue</c>
/// (PRI01) — the frontend finds "my vote" by looking up the current user's own ID in this
/// dictionary, rather than the API needing a per-request-user-scoped field.
/// </summary>
public record ProjectTaskDto(
    Guid Id,
    Guid ProjectId,
    Guid? PhaseId,
    Guid? EpicId,
    Guid? AssignedUserId,
    Guid? SecondaryAssigneeId,
    string Title,
    string? Description,
    string Status,
    string Priority,
    int WeightedScore,
    Dictionary<Guid, int> Votes,
    DateTime? DueDate,
    decimal? EstimatedHours,
    bool IsCompleted,
    Dictionary<string, string> ExternalMetadata,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
