using TaskManager.Application.Common.DTOs;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Common.Mappings;

public static class MappingExtensions
{
    public static ProjectDto ToDto(this Project p) => new(
        p.Id,
        p.Name,
        p.Description,
        p.Scope.ToString(),
        p.ColorHex,
        p.DueDate,
        p.IsArchived,
        p.CriticalityScore,
        p.CreatedByUserId,
        p.Audit.CreatedAt,
        p.Audit.UpdatedAt,
        p.Phases.Select(ph => ph.ToDto()).ToList(),
        p.Epics.Select(e => e.ToDto()).ToList(),
        p.Tasks.Select(t => t.ToDto()).ToList());

    public static ProjectPhaseDto ToDto(this ProjectPhase ph) => new(
        ph.Id,
        ph.ProjectId,
        ph.Name,
        ph.Description,
        ph.DisplayOrder,
        ph.Audit.CreatedAt,
        ph.Audit.UpdatedAt);

    public static EpicDto ToDto(this Epic e) => new(
        e.Id,
        e.ProjectId,
        e.Name,
        e.Description,
        e.ColorHex,
        e.DisplayOrder,
        e.Audit.CreatedAt,
        e.Audit.UpdatedAt);

    public static ProjectTaskDto ToDto(this ProjectTask t) => new(
        t.Id,
        t.ProjectId,
        t.PhaseId,
        t.EpicId,
        t.AssignedUserId,
        t.SecondaryAssigneeId,
        t.Title,
        t.Description,
        t.Status.ToString(),
        t.Priority.ToString(),
        t.WeightedScore,
        t.Votes.ToDictionary(v => v.UserId, v => v.VoteValue),
        t.DueDate,
        t.EstimatedHours,
        t.IsCompleted,
        t.ExternalMetadata,
        t.Audit.CreatedAt,
        t.Audit.UpdatedAt);

    public static AppUserDto ToDto(this AppUser u) => new(
        u.Id,
        u.Username,
        u.DisplayName,
        u.Email,
        u.AvatarUrl,
        u.IsActive,
        u.IsAdmin,
        u.Audit.CreatedAt,
        u.Audit.UpdatedAt);

    public static ApiTokenDto ToDto(this ApiToken t) => new(
        t.Id,
        t.Name,
        t.CreatedAt,
        t.CreatedByUserId,
        t.RevokedAt);

    public static AllowedOriginDto ToDto(this AllowedOrigin o) => new(
        o.Id,
        o.OriginUrl,
        o.CreatedAt);
}
