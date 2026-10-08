using Grpc.Net.Client;
using TaskManager.Grpc.Contracts;

namespace TaskManager.Mcp.GrpcClients;

public class AuthGrpcClient(GrpcChannel channel)
{
    private readonly AuthGrpcService.AuthGrpcServiceClient _client = new(channel);

    public async Task<ValidateTokenResponse> ValidateToken(string token) =>
        await _client.ValidateTokenAsync(new ValidateTokenRequest(), GrpcAuth.WithToken(token));
}
