using Grpc.Core;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using TaskManager.Application.Projects.Queries;
using Contracts = TaskManager.Grpc.Contracts;

namespace TaskManager.API.Grpc;

[Authorize]
public class ProjectsGrpcService : Contracts.ProjectsGrpcService.ProjectsGrpcServiceBase
{
    private readonly IMediator _mediator;

    public ProjectsGrpcService(IMediator mediator) => _mediator = mediator;

    public override async Task<Contracts.ListProjectsResponse> ListProjects(
        Contracts.ListProjectsRequest request, ServerCallContext context)
    {
        var projects = await _mediator.Send(new GetProjectsQuery(), context.CancellationToken);

        var response = new Contracts.ListProjectsResponse();
        response.Projects.AddRange(projects.Select(p => p.ToMessage()));
        return response;
    }

    public override async Task<Contracts.ProjectMessage> GetProject(
        Contracts.GetProjectRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Id must be a valid GUID."));

        var project = await _mediator.Send(new GetProjectByIdQuery(id), context.CancellationToken);
        if (project is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Project {request.Id} was not found."));

        return project.ToMessage();
    }
}
