using System.Security.Cryptography;
using MediatR;
using TaskManager.Application.Common.DTOs;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Common.Mappings;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.ApiTokens.Commands;

public record CreateApiTokenCommand(string Name, Guid CreatedByUserId) : IRequest<CreateApiTokenResult>;

/// <summary>The raw token is only ever returned here, at creation — never persisted, never shown again.</summary>
public record CreateApiTokenResult(string Token, ApiTokenDto ApiToken);

public class CreateApiTokenCommandHandler : IRequestHandler<CreateApiTokenCommand, CreateApiTokenResult>
{
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _hasher;

    public CreateApiTokenCommandHandler(IUnitOfWork uow, IPasswordHasher hasher)
    {
        _uow    = uow;
        _hasher = hasher;
    }

    public async Task<CreateApiTokenResult> Handle(CreateApiTokenCommand request, CancellationToken cancellationToken)
    {
        var rawToken  = RandomNumberGenerator.GetHexString(48);
        var tokenHash = _hasher.Hash(rawToken);

        var apiToken = new ApiToken(request.Name, tokenHash, request.CreatedByUserId);

        _uow.ApiTokens.Add(apiToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return new CreateApiTokenResult(rawToken, apiToken.ToDto());
    }
}
