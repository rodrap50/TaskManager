namespace TaskManager.Application.Common.DTOs;

public record AllowedOriginDto(
    Guid Id,
    string OriginUrl,
    DateTime CreatedAt);
