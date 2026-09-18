using Grpc.Core;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using TaskManager.Application.Phases.Queries;
using Contracts = TaskManager.Grpc.Contracts;

namespace TaskManager.API.Grpc;

[Authorize]
public class PhasesGrpcService : Contracts.PhasesGrpcService.PhasesGrpcServiceBase
{
    private readonly IMediator _mediator;

    public PhasesGrpcService(IMediator mediator) => _mediator = mediator;

    public override async Task<Contracts.ListPhasesByProjectResponse> ListPhasesByProject(
        Contracts.ListPhasesByProjectRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.ProjectId, out var projectId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "ProjectId must be a valid GUID."));

        var phases = await _mediator.Send(new GetPhasesByProjectQuery(projectId), context.CancellationToken);

        var response = new Contracts.ListPhasesByProjectResponse();
        response.Phases.AddRange(phases.Select(p => p.ToMessage()));
        return response;
    }
}
