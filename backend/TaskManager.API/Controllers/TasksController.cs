using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Common.DTOs;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Tasks.Commands;
using TaskManager.Application.Tasks.Queries;
using TaskManager.Domain.Enums;
using TaskStatus = TaskManager.Domain.Enums.TaskStatus;

namespace TaskManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<TasksController> _logger;

    public TasksController(IMediator mediator, ICurrentUserService currentUser, ILogger<TasksController> logger)
    {
        _mediator    = mediator;
        _currentUser = currentUser;
        _logger      = logger;
    }

    [HttpGet("project/{projectId:guid}")]
    public async Task<ActionResult<List<ProjectTaskDto>>> GetByProject(
        Guid projectId,
        [FromQuery] bool archived = false,
        [FromQuery] int? completedWithinDays = null,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetTasksByProjectQuery(projectId, archived, completedWithinDays), ct);
        return Ok(result);
    }

    [HttpPost("project/{projectId:guid}/archive-completed")]
    public async Task<ActionResult<ArchiveCompletedResponse>> ArchiveCompleted(
        Guid projectId,
        [FromBody] ArchiveCompletedRequest request,
        CancellationToken ct)
    {
        var count = await _mediator.Send(new ArchiveCompletedTasksCommand(projectId, request.OlderThanDays), ct);
        return Ok(new ArchiveCompletedResponse(count));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProjectTaskDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTaskByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ProjectTaskDto>> CreateTask(
        [FromBody] CreateTaskCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result is null ? NotFound() : CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProjectTaskDto>> UpdateTask(
        Guid id,
        [FromBody] UpdateTaskRequest request,
        CancellationToken ct)
    {
        var command = new UpdateTaskCommand(
            id,
            request.Title,
            request.Description,
            request.Priority,
            request.PhaseId,
            request.ClearPhase,
            request.EpicId,
            request.ClearEpic,
            request.AssignedUserId,
            request.ClearAssignee,
            request.SecondaryAssigneeId,
            request.ClearSecondaryAssignee,
            request.DueDate,
            request.EstimatedHours);

        var result = await _mediator.Send(command, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("{id:guid}/transition")]
    public async Task<ActionResult<ProjectTaskDto>> Transition(
        Guid id,
        [FromBody] TransitionRequest request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new TransitionTaskCommand(id, request.NewStatus), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("{id:guid}/archive")]
    public async Task<ActionResult<ProjectTaskDto>> Archive(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new ArchiveTaskCommand(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("{id:guid}/restore")]
    public async Task<ActionResult<ProjectTaskDto>> Restore(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new ArchiveTaskCommand(id, Restore: true), ct);
        return result is null ? NotFound() : Ok(result);
    }

    // Human admins only: API tokens carry no role claim, so they get 403 here — and there is
    // deliberately no gRPC/MCP equivalent.
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await _mediator.Send(new DeleteTaskCommand(id), ct);
        if (deleted is null) return NotFound();

        // ponytail: the structured log is the audit trail; move to an audit table once one exists.
        _logger.LogWarning(
            "AUDIT task deleted: {TaskId} \"{TaskTitle}\" (project {ProjectId}, status {Status}) by user {UserId}",
            deleted.Id, deleted.Title, deleted.ProjectId, deleted.Status, _currentUser.UserId);

        return NoContent();
    }

    [HttpPut("{id:guid}/votes")]
    public async Task<ActionResult<ProjectTaskDto>> CastVote(
        Guid id,
        [FromBody] CastVoteRequest request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new CastVoteCommand(id, _currentUser.UserId, request.VoteValue), ct);
        return result is null ? NotFound() : Ok(result);
    }
}

public record UpdateTaskRequest(
    string? Title,
    string? Description,
    TaskPriority? Priority,
    Guid? PhaseId,
    bool ClearPhase,
    Guid? EpicId,
    bool ClearEpic,
    Guid? AssignedUserId,
    bool ClearAssignee,
    Guid? SecondaryAssigneeId,
    bool ClearSecondaryAssignee,
    DateTime? DueDate,
    decimal? EstimatedHours);

public record TransitionRequest(TaskStatus NewStatus);

public record CastVoteRequest(int VoteValue);

public record ArchiveCompletedRequest(int OlderThanDays);

public record ArchiveCompletedResponse(int ArchivedCount);
