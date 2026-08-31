using MediatR;
using TaskManager.Application.Common.DTOs;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Common.Mappings;

namespace TaskManager.Application.Users.Queries;

public record GetUserByIdQuery(Guid Id) : IRequest<AppUserDto?>;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, AppUserDto?>
{
    private readonly IUnitOfWork _uow;

    public GetUserByIdQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<AppUserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _uow.Users.GetByIdAsync(request.Id, cancellationToken);
        return user?.ToDto();
    }
}
