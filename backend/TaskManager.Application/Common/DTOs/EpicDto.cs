namespace TaskManager.Application.Common.DTOs;

public record EpicDto(
    Guid Id,
    Guid ProjectId,
    string Name,
    string? Description,
    string? ColorHex,
    int DisplayOrder,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
