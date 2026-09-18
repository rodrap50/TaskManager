using Google.Protobuf.WellKnownTypes;
using TaskManager.Application.Common.DTOs;
using TaskManager.Domain.Enums;
using Contracts = TaskManager.Grpc.Contracts;
using DomainTaskStatus = TaskManager.Domain.Enums.TaskStatus;

namespace TaskManager.API.Grpc;

/// <summary>
/// Maps Application-layer DTOs to their generated gRPC message counterparts. Shared by every
/// *GrpcService (MCP01.3/MCP01.4) so the mapping rules for a given DTO live in exactly one place.
/// </summary>
internal static class GrpcMappingExtensions
{
    public static Contracts.ProjectMessage ToMessage(this ProjectDto dto)
    {
        var message = new Contracts.ProjectMessage
        {
            Id                = dto.Id.ToString(),
            Name              = dto.Name,
            Scope             = System.Enum.Parse<ProjectScope>(dto.Scope).ToGrpc(),
            IsArchived        = dto.IsArchived,
            CriticalityScore  = dto.CriticalityScore,
            CreatedByUserId   = dto.CreatedByUserId.ToString(),
            CreatedAt         = dto.CreatedAt.ToUtcTimestamp(),
        };

        if (dto.Description is not null) message.Description = dto.Description;
        if (dto.ColorHex is not null) message.ColorHex = dto.ColorHex;
        if (dto.DueDate is { } dueDate) message.DueDate = dueDate.ToUtcTimestamp();
        if (dto.UpdatedAt is { } updatedAt) message.UpdatedAt = updatedAt.ToUtcTimestamp();

        message.Phases.AddRange(dto.Phases.Select(p => p.ToMessage()));
        message.Epics.AddRange(dto.Epics.Select(e => e.ToMessage()));
        message.Tasks.AddRange(dto.Tasks.Select(t => t.ToMessage()));

        return message;
    }

    public static Contracts.EpicMessage ToMessage(this EpicDto dto)
    {
        var message = new Contracts.EpicMessage
        {
            Id            = dto.Id.ToString(),
            ProjectId     = dto.ProjectId.ToString(),
            Name          = dto.Name,
            DisplayOrder  = dto.DisplayOrder,
            CreatedAt     = dto.CreatedAt.ToUtcTimestamp(),
        };

        if (dto.Description is not null) message.Description = dto.Description;
        if (dto.ColorHex is not null) message.ColorHex = dto.ColorHex;
        if (dto.UpdatedAt is { } updatedAt) message.UpdatedAt = updatedAt.ToUtcTimestamp();

        return message;
    }

    public static Contracts.ProjectPhaseMessage ToMessage(this ProjectPhaseDto dto)
    {
        var message = new Contracts.ProjectPhaseMessage
        {
            Id            = dto.Id.ToString(),
            ProjectId     = dto.ProjectId.ToString(),
            Name          = dto.Name,
            DisplayOrder  = dto.DisplayOrder,
            CreatedAt     = dto.CreatedAt.ToUtcTimestamp(),
        };

        if (dto.Description is not null) message.Description = dto.Description;
        if (dto.UpdatedAt is { } updatedAt) message.UpdatedAt = updatedAt.ToUtcTimestamp();

        return message;
    }

    public static Contracts.ProjectTaskMessage ToMessage(this ProjectTaskDto dto)
    {
        var message = new Contracts.ProjectTaskMessage
        {
            Id              = dto.Id.ToString(),
            ProjectId       = dto.ProjectId.ToString(),
            Title           = dto.Title,
            Status          = System.Enum.Parse<DomainTaskStatus>(dto.Status).ToGrpc(),
            Priority        = System.Enum.Parse<TaskPriority>(dto.Priority).ToGrpc(),
            WeightedScore   = dto.WeightedScore,
            IsCompleted     = dto.IsCompleted,
            CreatedAt       = dto.CreatedAt.ToUtcTimestamp(),
        };

        if (dto.PhaseId is { } phaseId) message.PhaseId = phaseId.ToString();
        if (dto.EpicId is { } epicId) message.EpicId = epicId.ToString();
        if (dto.AssignedUserId is { } assignedUserId) message.AssignedUserId = assignedUserId.ToString();
        if (dto.SecondaryAssigneeId is { } secondaryAssigneeId) message.SecondaryAssigneeId = secondaryAssigneeId.ToString();
        if (dto.Description is not null) message.Description = dto.Description;
        if (dto.DueDate is { } dueDate) message.DueDate = dueDate.ToUtcTimestamp();
        if (dto.EstimatedHours is { } estimatedHours) message.EstimatedHours = (double)estimatedHours;
        if (dto.UpdatedAt is { } updatedAt) message.UpdatedAt = updatedAt.ToUtcTimestamp();

        foreach (var (userId, voteValue) in dto.Votes)
            message.Votes[userId.ToString()] = voteValue;

        foreach (var (key, value) in dto.ExternalMetadata)
            message.ExternalMetadata[key] = value;

        return message;
    }

    public static Contracts.ProjectScopeValue ToGrpc(this ProjectScope scope) => scope switch
    {
        ProjectScope.Project   => Contracts.ProjectScopeValue.ProjectScopeProject,
        ProjectScope.DailyTask => Contracts.ProjectScopeValue.ProjectScopeDailyTask,
        _ => throw new ArgumentOutOfRangeException(nameof(scope), scope, null),
    };

    public static Contracts.TaskStatusValue ToGrpc(this DomainTaskStatus status) => status switch
    {
        DomainTaskStatus.Backlog    => Contracts.TaskStatusValue.TaskStatusBacklog,
        DomainTaskStatus.InProgress => Contracts.TaskStatusValue.TaskStatusInProgress,
        DomainTaskStatus.Blocked    => Contracts.TaskStatusValue.TaskStatusBlocked,
        DomainTaskStatus.Done       => Contracts.TaskStatusValue.TaskStatusDone,
        DomainTaskStatus.Cancelled  => Contracts.TaskStatusValue.TaskStatusCancelled,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
    };

    public static Contracts.TaskPriorityValue ToGrpc(this TaskPriority priority) => priority switch
    {
        TaskPriority.Low      => Contracts.TaskPriorityValue.TaskPriorityLow,
        TaskPriority.Medium   => Contracts.TaskPriorityValue.TaskPriorityMedium,
        TaskPriority.High     => Contracts.TaskPriorityValue.TaskPriorityHigh,
        TaskPriority.Critical => Contracts.TaskPriorityValue.TaskPriorityCritical,
        _ => throw new ArgumentOutOfRangeException(nameof(priority), priority, null),
    };

    public static DomainTaskStatus ToDomain(this Contracts.TaskStatusValue status) => status switch
    {
        Contracts.TaskStatusValue.TaskStatusBacklog    => DomainTaskStatus.Backlog,
        Contracts.TaskStatusValue.TaskStatusInProgress => DomainTaskStatus.InProgress,
        Contracts.TaskStatusValue.TaskStatusBlocked    => DomainTaskStatus.Blocked,
        Contracts.TaskStatusValue.TaskStatusDone       => DomainTaskStatus.Done,
        Contracts.TaskStatusValue.TaskStatusCancelled  => DomainTaskStatus.Cancelled,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
    };

    public static TaskPriority ToDomain(this Contracts.TaskPriorityValue priority) => priority switch
    {
        Contracts.TaskPriorityValue.TaskPriorityLow      => TaskPriority.Low,
        Contracts.TaskPriorityValue.TaskPriorityMedium   => TaskPriority.Medium,
        Contracts.TaskPriorityValue.TaskPriorityHigh     => TaskPriority.High,
        Contracts.TaskPriorityValue.TaskPriorityCritical => TaskPriority.Critical,
        _ => throw new ArgumentOutOfRangeException(nameof(priority), priority, null),
    };

    // EF/Npgsql round-trips DateTime as Kind=Unspecified; every DateTime in this app is UTC by
    // convention (DateTime.UtcNow at the point of creation), so it's safe to stamp the Kind
    // rather than convert — Timestamp.FromDateTime throws unless Kind is explicitly Utc.
    public static Timestamp ToUtcTimestamp(this DateTime dateTime) =>
        Timestamp.FromDateTime(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc));
}
