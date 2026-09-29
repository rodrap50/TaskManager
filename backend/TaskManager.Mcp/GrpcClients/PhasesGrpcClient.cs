using Grpc.Net.Client;
using TaskManager.Grpc.Contracts;

namespace TaskManager.Mcp.GrpcClients;

public class PhasesGrpcClient(GrpcChannel channel)
{
    private readonly PhasesGrpcService.PhasesGrpcServiceClient _client = new(channel);

    public async Task<ListPhasesByProjectResponse> ListPhasesByProject(string token, string projectId) =>
        await _client.ListPhasesByProjectAsync(
            new ListPhasesByProjectRequest { ProjectId = projectId }, GrpcAuth.WithToken(token));
}
