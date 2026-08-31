using MediatR;
using TaskManager.Application.Common.Interfaces;

namespace TaskManager.Application.Auth.Queries;

public record GetSetupStatusQuery : IRequest<SetupStatusDto>;

public record SetupStatusDto(bool SetupRequired);

public class GetSetupStatusQueryHandler : IRequestHandler<GetSetupStatusQuery, SetupStatusDto>
{
    private readonly IUnitOfWork _uow;

    public GetSetupStatusQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<SetupStatusDto> Handle(GetSetupStatusQuery request, CancellationToken cancellationToken)
    {
        var adminExists = await _uow.Users.ExistsAdminAsync(cancellationToken);
        return new SetupStatusDto(SetupRequired: !adminExists);
    }
}
