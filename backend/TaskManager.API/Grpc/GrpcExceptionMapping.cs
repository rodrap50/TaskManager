using FluentValidation;
using Grpc.Core;

namespace TaskManager.API.Grpc;

/// <summary>
/// Maps the same Application-layer exceptions Program.cs's REST exception handler maps
/// (ValidationException → 400, InvalidOperationException → 422) to their gRPC-status
/// equivalents, so a mutating RPC fails cleanly instead of surfacing as a generic Unknown
/// status. Read-only RPCs (List*/Get*) don't invoke commands and don't need this.
/// </summary>
internal static class GrpcExceptionMapping
{
    public static async Task<TResponse> ExecuteAsync<TResponse>(Func<Task<TResponse>> action)
    {
        try
        {
            return await action();
        }
        catch (ValidationException ex)
        {
            var detail = string.Join("; ", ex.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
            throw new RpcException(new Status(StatusCode.InvalidArgument, detail));
        }
        catch (InvalidOperationException ex)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
        }
    }
}
