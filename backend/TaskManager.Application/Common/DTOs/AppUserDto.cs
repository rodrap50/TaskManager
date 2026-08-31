namespace TaskManager.Application.Common.DTOs;

public record AppUserDto(
    Guid Id,
    string Username,
    string DisplayName,
    string Email,
    string? AvatarUrl,
    bool IsActive,
    bool IsAdmin,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
