namespace TaskManager.Application.Common.DTOs;

public record ProjectPhaseDto(
    Guid Id,
    Guid ProjectId,
    string Name,
    string? Description,
    int DisplayOrder,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
