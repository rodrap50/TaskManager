using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Contracts = TaskManager.Grpc.Contracts;

namespace TaskManager.API.Grpc;

/// <summary>
/// Lets TaskManager.Mcp's agent-plan tools (which never otherwise call the API) check a
/// caller's token (MCP08). [Authorize] runs it through the same SmartAuth/ApiToken
/// pipeline as every other RPC, so expiry/revocation live in one place; the read-only
/// flag is the claim ApiTokenAuthenticationHandler already sets.
/// </summary>
[Authorize]
public class AuthGrpcService : Contracts.AuthGrpcService.AuthGrpcServiceBase
{
    public override Task<Contracts.ValidateTokenResponse> ValidateToken(
        Contracts.ValidateTokenRequest request, ServerCallContext context) =>
        Task.FromResult(new Contracts.ValidateTokenResponse
        {
            IsReadOnly = context.GetHttpContext().User.FindFirst("isReadOnly")?.Value == "true",
        });
}
