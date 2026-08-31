using MediatR;
using TaskManager.Application.Common.DTOs;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Common.Mappings;

namespace TaskManager.Application.Projects.Members.Queries;

public record GetProjectMembersQuery(Guid ProjectId) : IRequest<List<AppUserDto>>;

public class GetProjectMembersQueryHandler : IRequestHandler<GetProjectMembersQuery, List<AppUserDto>>
{
    private readonly IUnitOfWork _uow;

    public GetProjectMembersQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<List<AppUserDto>> Handle(GetProjectMembersQuery request, CancellationToken cancellationToken)
    {
        var members = await _uow.Users.GetByProjectAsync(request.ProjectId, cancellationToken);
        return members.Select(u => u.ToDto()).ToList();
    }
}
