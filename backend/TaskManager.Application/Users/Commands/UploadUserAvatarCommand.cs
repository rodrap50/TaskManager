using MediatR;
using TaskManager.Application.Common.DTOs;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Common.Mappings;

namespace TaskManager.Application.Users.Commands;

public record UploadUserAvatarCommand(Guid Id, string Extension, Stream Content) : IRequest<AppUserDto?>;

public class UploadUserAvatarCommandHandler : IRequestHandler<UploadUserAvatarCommand, AppUserDto?>
{
    private readonly IUnitOfWork _uow;
    private readonly IAvatarStorage _storage;

    public UploadUserAvatarCommandHandler(IUnitOfWork uow, IAvatarStorage storage)
    {
        _uow     = uow;
        _storage = storage;
    }

    public async Task<AppUserDto?> Handle(UploadUserAvatarCommand request, CancellationToken cancellationToken)
    {
        var user = await _uow.Users.GetByIdAsync(request.Id, cancellationToken);
        if (user is null) return null;

        var avatarUrl = await _storage.SaveAsync(request.Id, request.Extension, request.Content, cancellationToken);
        user.UpdateAvatarUrl(avatarUrl);

        await _uow.SaveChangesAsync(cancellationToken);

        return user.ToDto();
    }
}
