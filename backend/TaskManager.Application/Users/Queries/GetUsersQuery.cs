using MediatR;
using TaskManager.Application.Common.DTOs;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Common.Mappings;

namespace TaskManager.Application.Users.Queries;

public record GetUsersQuery() : IRequest<List<AppUserDto>>;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<AppUserDto>>
{
    private readonly IUnitOfWork _uow;

    public GetUsersQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<List<AppUserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _uow.Users.GetAllAsync(cancellationToken);
        return users.Select(u => u.ToDto()).ToList();
    }
}
