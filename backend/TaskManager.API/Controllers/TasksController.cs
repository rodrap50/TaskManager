using MediatR;
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

    public TasksController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator    = mediator;
        _currentUser = currentUser;
    }

    [HttpGet("project/{projectId:guid}")]
    public async Task<ActionResult<List<ProjectTaskDto>>> GetByProject(Guid projectId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTasksByProjectQuery(projectId), ct);
        return Ok(result);
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
