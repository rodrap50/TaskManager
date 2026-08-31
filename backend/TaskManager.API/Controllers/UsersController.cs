using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Common.DTOs;
using TaskManager.Application.Users.Commands;
using TaskManager.Application.Users.Queries;

namespace TaskManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private const long MaxAvatarBytes = 2 * 1024 * 1024;

    private static readonly Dictionary<string, string> AllowedAvatarContentTypes = new()
    {
        ["image/png"]  = ".png",
        ["image/jpeg"] = ".jpg",
        ["image/webp"] = ".webp",
        ["image/gif"]  = ".gif",
    };

    private readonly IMediator _mediator;

    public UsersController(IMediator mediator) => _mediator = mediator;

    // Intentionally open to any authenticated user (not admin-only): this list is the
    // lookup source for assignee pickers (DS04, U02.4) and project-membership search
    // (PM02) across the regular app, not just an admin-management view.
    [HttpGet]
    public async Task<ActionResult<List<AppUserDto>>> GetUsers(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUsersQuery(), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AppUserDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUserByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<AppUserDto>> CreateUser(
        [FromBody] CreateUserCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}/active")]
    public async Task<ActionResult<AppUserDto>> SetActive(
        Guid id, [FromBody] SetActiveRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new SetUserActiveCommand(id, request.IsActive), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}/admin")]
    public async Task<ActionResult<AppUserDto>> SetAdmin(
        Guid id, [FromBody] SetAdminRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new SetUserAdminCommand(id, request.IsAdmin), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/avatar")]
    [RequestSizeLimit(MaxAvatarBytes)]
    public async Task<ActionResult<AppUserDto>> UploadAvatar(Guid id, IFormFile? file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "An image file is required." });

        if (file.Length > MaxAvatarBytes)
            return BadRequest(new { error = "Avatar image must be 2MB or smaller." });

        if (!AllowedAvatarContentTypes.TryGetValue(file.ContentType, out var extension))
            return BadRequest(new { error = "Avatar must be a PNG, JPEG, WEBP, or GIF image." });

        await using var stream = file.OpenReadStream();
        var result = await _mediator.Send(new UploadUserAvatarCommand(id, extension, stream), ct);
        return result is null ? NotFound() : Ok(result);
    }
}

public record SetActiveRequest(bool IsActive);
public record SetAdminRequest(bool IsAdmin);
