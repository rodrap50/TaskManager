using Grpc.Core;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using TaskManager.Application.Tasks.Commands;
using TaskManager.Application.Tasks.Queries;
using TaskManager.Domain.Services;
using Contracts = TaskManager.Grpc.Contracts;

namespace TaskManager.API.Grpc;

[Authorize]
public class TasksGrpcService : Contracts.TasksGrpcService.TasksGrpcServiceBase
{
    private readonly IMediator _mediator;

    public TasksGrpcService(IMediator mediator) => _mediator = mediator;

    public override async Task<Contracts.ListTasksByProjectResponse> ListTasksByProject(
        Contracts.ListTasksByProjectRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.ProjectId, out var projectId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "ProjectId must be a valid GUID."));

        var query = new GetTasksByProjectQuery(
            projectId, request.Archived, request.HasCompletedWithinDays ? request.CompletedWithinDays : null);
        var tasks = await GrpcExceptionMapping.ExecuteAsync(() => _mediator.Send(query, context.CancellationToken));

        var response = new Contracts.ListTasksByProjectResponse();
        response.Tasks.AddRange(tasks.Select(t => t.ToMessage()));
        return response;
    }

    public override async Task<Contracts.ProjectTaskMessage> GetTask(
        Contracts.GetTaskRequest request, ServerCallContext context)
    {
        IRequest<Application.Common.DTOs.ProjectTaskDto?> query;
        if (Guid.TryParse(request.Id, out var id))
            query = new GetTaskByIdQuery(id);
        else if (ProjectKeys.TryParseDisplayId(request.Id, out _, out _))
            query = new GetTaskByDisplayIdQuery(request.Id);
        else
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Id must be a GUID or a display ID such as TSKM-001."));

        var task = await _mediator.Send(query, context.CancellationToken);
        if (task is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Task {request.Id} was not found."));

        return task.ToMessage();
    }

    public override async Task<Contracts.ProjectTaskMessage> CreateTask(
        Contracts.CreateTaskRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.ProjectId, out var projectId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "ProjectId must be a valid GUID."));

        if (request.HasPhaseId && !Guid.TryParse(request.PhaseId, out _))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "PhaseId must be a valid GUID."));
        if (request.HasEpicId && !Guid.TryParse(request.EpicId, out _))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "EpicId must be a valid GUID."));
        if (request.HasAssignedUserId && !Guid.TryParse(request.AssignedUserId, out _))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "AssignedUserId must be a valid GUID."));

        var command = new CreateTaskCommand(
            projectId,
            request.Title,
            request.Priority.ToDomain(),
            request.HasDescription ? request.Description : null,
            request.HasPhaseId ? Guid.Parse(request.PhaseId) : null,
            request.HasEpicId ? Guid.Parse(request.EpicId) : null,
            request.HasAssignedUserId ? Guid.Parse(request.AssignedUserId) : null,
            request.DueDate is not null ? request.DueDate.ToDateTime() : null,
            request.HasEstimatedHours ? (decimal)request.EstimatedHours : null);

        var task = await GrpcExceptionMapping.ExecuteAsync(() => _mediator.Send(command, context.CancellationToken));
        if (task is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Project {request.ProjectId} was not found."));

        return task.ToMessage();
    }

    public override async Task<Contracts.ProjectTaskMessage> UpdateTask(
        Contracts.UpdateTaskRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Id must be a valid GUID."));

        if (request.HasPhaseId && !Guid.TryParse(request.PhaseId, out _))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "PhaseId must be a valid GUID."));
        if (request.HasEpicId && !Guid.TryParse(request.EpicId, out _))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "EpicId must be a valid GUID."));
        if (request.HasAssignedUserId && !Guid.TryParse(request.AssignedUserId, out _))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "AssignedUserId must be a valid GUID."));
        if (request.HasSecondaryAssigneeId && !Guid.TryParse(request.SecondaryAssigneeId, out _))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "SecondaryAssigneeId must be a valid GUID."));

        var command = new UpdateTaskCommand(
            id,
            request.HasTitle ? request.Title : null,
            request.HasDescription ? request.Description : null,
            request.HasPriority ? request.Priority.ToDomain() : null,
            request.HasPhaseId ? Guid.Parse(request.PhaseId) : null,
            request.ClearPhase,
            request.HasEpicId ? Guid.Parse(request.EpicId) : null,
            request.ClearEpic,
            request.HasAssignedUserId ? Guid.Parse(request.AssignedUserId) : null,
            request.ClearAssignee,
            request.HasSecondaryAssigneeId ? Guid.Parse(request.SecondaryAssigneeId) : null,
            request.ClearSecondaryAssignee,
            request.DueDate is not null ? request.DueDate.ToDateTime() : null,
            request.HasEstimatedHours ? (decimal)request.EstimatedHours : null);

        var task = await GrpcExceptionMapping.ExecuteAsync(() => _mediator.Send(command, context.CancellationToken));
        if (task is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Task {request.Id} was not found."));

        return task.ToMessage();
    }

    public override async Task<Contracts.ProjectTaskMessage> TransitionTask(
        Contracts.TransitionTaskRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Id must be a valid GUID."));

        var command = new TransitionTaskCommand(id, request.NewStatus.ToDomain());

        var task = await GrpcExceptionMapping.ExecuteAsync(() => _mediator.Send(command, context.CancellationToken));
        if (task is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Task {request.Id} was not found."));

        return task.ToMessage();
    }

    public override Task<Contracts.ProjectTaskMessage> ArchiveTask(
        Contracts.ArchiveTaskRequest request, ServerCallContext context)
        => SendArchive(request.Id, restore: false, context);

    public override Task<Contracts.ProjectTaskMessage> RestoreTask(
        Contracts.RestoreTaskRequest request, ServerCallContext context)
        => SendArchive(request.Id, restore: true, context);

    private async Task<Contracts.ProjectTaskMessage> SendArchive(string rawId, bool restore, ServerCallContext context)
    {
        if (!Guid.TryParse(rawId, out var id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Id must be a valid GUID."));

        var command = new ArchiveTaskCommand(id, restore);

        var task = await GrpcExceptionMapping.ExecuteAsync(() => _mediator.Send(command, context.CancellationToken));
        if (task is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Task {rawId} was not found."));

        return task.ToMessage();
    }
}
