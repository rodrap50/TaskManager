namespace TaskManager.Application.Common.DTOs;

/// <summary>
/// <paramref name="Votes"/> maps every voter's <c>UserId</c> to their cast <c>VoteValue</c>
/// (PRI01) — the frontend finds "my vote" by looking up the current user's own ID in this
/// dictionary, rather than the API needing a per-request-user-scoped field.
/// <paramref name="DisplayId"/> is the human-readable ticket ID ("TSKM-001"); it is
/// <see langword="null"/> only if the task's project wasn't loaded.
/// </summary>
public record ProjectTaskDto(
    Guid Id,
    int Number,
    string? DisplayId,
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
    DateTime? UpdatedAt,
    DateTime? CompletedAt,
    bool IsArchived,
    DateTime? ArchivedAt);
