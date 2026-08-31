namespace TaskManager.Application.Common.DTOs;

public record ApiTokenDto(
    Guid Id,
    string Name,
    DateTime CreatedAt,
    Guid CreatedByUserId,
    DateTime? RevokedAt);
