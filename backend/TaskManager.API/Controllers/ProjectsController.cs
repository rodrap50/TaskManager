using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Common.DTOs;
using TaskManager.Application.Projects.Commands;
using TaskManager.Application.Projects.Members.Commands;
using TaskManager.Application.Projects.Members.Queries;
using TaskManager.Application.Projects.Queries;
using TaskManager.Domain.Enums;

namespace TaskManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProjectsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<List<ProjectDto>>> GetProjects(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProjectsQuery(), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProjectDto>> GetProjectById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProjectByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ProjectDto>> CreateProject(
        [FromBody] CreateProjectCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetProjectById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProjectDto>> UpdateProject(
        Guid id,
        [FromBody] UpdateProjectRequest request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new UpdateProjectCommand(id, request.Name, request.Description, request.Scope, request.ColorHex, request.DueDate), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}/criticality-score")]
    public async Task<ActionResult<ProjectDto>> SetCriticalityScore(
        Guid id, [FromBody] SetCriticalityScoreRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new SetProjectCriticalityScoreCommand(id, request.CriticalityScore), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("{id:guid}/archive")]
    public async Task<ActionResult<ProjectDto>> ArchiveProject(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new ArchiveProjectCommand(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("{id:guid}/restore")]
    public async Task<ActionResult<ProjectDto>> RestoreProject(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new ArchiveProjectCommand(id, Restore: true), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("{projectId:guid}/members")]
    public async Task<ActionResult<AppUserDto>> AddMember(
        Guid projectId, [FromBody] AddMemberRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new AddProjectMemberCommand(projectId, request.UserId), ct);
        return result.Outcome == AddProjectMemberOutcome.Success ? Ok(result.Member) : NotFound();
    }

    [HttpDelete("{projectId:guid}/members/{userId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid projectId, Guid userId, CancellationToken ct)
    {
        var found = await _mediator.Send(new RemoveProjectMemberCommand(projectId, userId), ct);
        return found ? NoContent() : NotFound();
    }

    [HttpGet("{projectId:guid}/members")]
    public async Task<ActionResult<List<AppUserDto>>> GetMembers(Guid projectId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProjectMembersQuery(projectId), ct);
        return Ok(result);
    }
}

public record UpdateProjectRequest(
    string? Name,
    string? Description,
    ProjectScope? Scope,
    string? ColorHex,
    DateTime? DueDate);

public record AddMemberRequest(Guid UserId);
public record SetCriticalityScoreRequest(int CriticalityScore);
