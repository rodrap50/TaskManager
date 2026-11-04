using MediatR;
using TaskManager.Application.Common.Interfaces;

namespace TaskManager.Application.Tasks.Commands;

/// <summary>
/// Archives every Done/Cancelled task completed more than <paramref name="OlderThanDays"/> days ago.
/// Returns how many were archived. <paramref name="ProjectId"/> <see langword="null"/> covers every
/// project (used by the automatic archiving job).
/// </summary>
public record ArchiveCompletedTasksCommand(Guid? ProjectId, int OlderThanDays) : IRequest<int>;

public class ArchiveCompletedTasksCommandHandler : IRequestHandler<ArchiveCompletedTasksCommand, int>
{
    private readonly IUnitOfWork _uow;

    public ArchiveCompletedTasksCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<int> Handle(ArchiveCompletedTasksCommand request, CancellationToken cancellationToken)
    {
        var cutoff = DateTime.UtcNow.AddDays(-request.OlderThanDays);
        var tasks = await _uow.Tasks.GetCompletedBeforeAsync(request.ProjectId, cutoff, cancellationToken);
        if (tasks.Count == 0) return 0;

        foreach (var task in tasks)
            task.Archive();

        await _uow.SaveChangesAsync(cancellationToken);
        return tasks.Count;
    }
}
