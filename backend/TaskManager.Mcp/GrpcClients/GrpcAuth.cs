using Grpc.Core;

namespace TaskManager.Mcp.GrpcClients;

/// <summary>
/// Builds call options carrying the caller's ApiToken (MCP02) as gRPC metadata, using the
/// same header name TaskManager.API's SmartAuth scheme checks for on the way in
/// (Grpc.Net.Client.CallCredentials is silently dropped on insecure/h2c channels — plain
/// metadata headers are the only thing that reliably reaches the server).
/// </summary>
internal static class GrpcAuth
{
    private const string ApiTokenHeaderName = "x-api-token";

    public static CallOptions WithToken(string token) =>
        new(headers: new Metadata { { ApiTokenHeaderName, token } });
}
