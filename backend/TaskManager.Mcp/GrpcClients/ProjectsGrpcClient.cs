using Grpc.Net.Client;
using TaskManager.Grpc.Contracts;

namespace TaskManager.Mcp.GrpcClients;

public class ProjectsGrpcClient(GrpcChannel channel)
{
    private readonly ProjectsGrpcService.ProjectsGrpcServiceClient _client = new(channel);

    public async Task<ListProjectsResponse> ListProjects(string token) =>
        await _client.ListProjectsAsync(new ListProjectsRequest(), GrpcAuth.WithToken(token));

    public async Task<ProjectMessage> GetProject(string token, string id) =>
        await _client.GetProjectAsync(new GetProjectRequest { Id = id }, GrpcAuth.WithToken(token));
}
