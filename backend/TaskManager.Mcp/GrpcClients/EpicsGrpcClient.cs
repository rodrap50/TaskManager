using Grpc.Net.Client;
using TaskManager.Grpc.Contracts;

namespace TaskManager.Mcp.GrpcClients;

public class EpicsGrpcClient(GrpcChannel channel)
{
    private readonly EpicsGrpcService.EpicsGrpcServiceClient _client = new(channel);

    public async Task<ListEpicsByProjectResponse> ListEpicsByProject(string token, string projectId) =>
        await _client.ListEpicsByProjectAsync(
            new ListEpicsByProjectRequest { ProjectId = projectId }, GrpcAuth.WithToken(token));

    public async Task<EpicMessage> CreateEpic(string token, CreateEpicRequest request) =>
        await _client.CreateEpicAsync(request, GrpcAuth.WithToken(token));

    public async Task<EpicMessage> UpdateEpic(string token, UpdateEpicRequest request) =>
        await _client.UpdateEpicAsync(request, GrpcAuth.WithToken(token));
}
