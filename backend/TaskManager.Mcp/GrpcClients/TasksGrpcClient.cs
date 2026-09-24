using Grpc.Net.Client;
using TaskManager.Grpc.Contracts;

namespace TaskManager.Mcp.GrpcClients;

public class TasksGrpcClient(GrpcChannel channel)
{
    private readonly TasksGrpcService.TasksGrpcServiceClient _client = new(channel);

    public async Task<ListTasksByProjectResponse> ListTasksByProject(string token, string projectId) =>
        await _client.ListTasksByProjectAsync(
            new ListTasksByProjectRequest { ProjectId = projectId }, GrpcAuth.WithToken(token));

    public async Task<ProjectTaskMessage> GetTask(string token, string id) =>
        await _client.GetTaskAsync(new GetTaskRequest { Id = id }, GrpcAuth.WithToken(token));

    public async Task<ProjectTaskMessage> CreateTask(string token, CreateTaskRequest request) =>
        await _client.CreateTaskAsync(request, GrpcAuth.WithToken(token));

    public async Task<ProjectTaskMessage> UpdateTask(string token, UpdateTaskRequest request) =>
        await _client.UpdateTaskAsync(request, GrpcAuth.WithToken(token));

    public async Task<ProjectTaskMessage> TransitionTask(string token, string id, TaskStatusValue newStatus) =>
        await _client.TransitionTaskAsync(
            new TransitionTaskRequest { Id = id, NewStatus = newStatus }, GrpcAuth.WithToken(token));
}
