namespace TaskManager.Application.Common.DTOs;

public record ProjectDto(
    Guid Id,
    string Name,
    string? Description,
    string Scope,
    string? ColorHex,
    DateTime? DueDate,
    bool IsArchived,
    int CriticalityScore,
    Guid CreatedByUserId,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<ProjectPhaseDto> Phases,
    IReadOnlyList<EpicDto> Epics,
    IReadOnlyList<ProjectTaskDto> Tasks);
