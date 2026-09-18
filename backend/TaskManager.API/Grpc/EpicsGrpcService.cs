using Grpc.Core;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using TaskManager.Application.Epics.Commands;
using TaskManager.Application.Epics.Queries;
using Contracts = TaskManager.Grpc.Contracts;

namespace TaskManager.API.Grpc;

[Authorize]
public class EpicsGrpcService : Contracts.EpicsGrpcService.EpicsGrpcServiceBase
{
    private readonly IMediator _mediator;

    public EpicsGrpcService(IMediator mediator) => _mediator = mediator;

    public override async Task<Contracts.ListEpicsByProjectResponse> ListEpicsByProject(
        Contracts.ListEpicsByProjectRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.ProjectId, out var projectId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "ProjectId must be a valid GUID."));

        var epics = await _mediator.Send(new GetEpicsByProjectQuery(projectId), context.CancellationToken);

        var response = new Contracts.ListEpicsByProjectResponse();
        response.Epics.AddRange(epics.Select(e => e.ToMessage()));
        return response;
    }

    public override async Task<Contracts.EpicMessage> CreateEpic(
        Contracts.CreateEpicRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.ProjectId, out var projectId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "ProjectId must be a valid GUID."));

        var command = new CreateEpicCommand(
            projectId,
            request.Name,
            request.DisplayOrder,
            request.HasDescription ? request.Description : null,
            request.HasColorHex ? request.ColorHex : null);

        var epic = await GrpcExceptionMapping.ExecuteAsync(() => _mediator.Send(command, context.CancellationToken));
        if (epic is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Project {request.ProjectId} was not found."));

        return epic.ToMessage();
    }

    public override async Task<Contracts.EpicMessage> UpdateEpic(
        Contracts.UpdateEpicRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Id must be a valid GUID."));

        var command = new UpdateEpicCommand(
            id,
            request.HasName ? request.Name : null,
            request.HasDescription ? request.Description : null,
            request.HasDisplayOrder ? request.DisplayOrder : null);

        var epic = await GrpcExceptionMapping.ExecuteAsync(() => _mediator.Send(command, context.CancellationToken));
        if (epic is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Epic {request.Id} was not found."));

        return epic.ToMessage();
    }
}
