using MediatR;
using TaskManager.Application.Common.DTOs;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Application.Common.Mappings;

namespace TaskManager.Application.Projects.Commands;

public record SetProjectCriticalityScoreCommand(Guid ProjectId, int CriticalityScore) : IRequest<ProjectDto?>;

public class SetProjectCriticalityScoreCommandHandler : IRequestHandler<SetProjectCriticalityScoreCommand, ProjectDto?>
{
    private readonly IUnitOfWork _uow;

    public SetProjectCriticalityScoreCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<ProjectDto?> Handle(SetProjectCriticalityScoreCommand request, CancellationToken cancellationToken)
    {
        var project = await _uow.Projects.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project is null) return null;

        project.SetCriticalityScore(request.CriticalityScore);

        // Recalculate every task in the project, not just newly-touched ones — this is the
        // one path (unlike a single vote cast or priority change) where the formula's input
        // changed for the whole project at once.
        var tasks = await _uow.Tasks.GetTrackedByProjectAsync(request.ProjectId, cancellationToken);
        foreach (var task in tasks)
            task.RecalculateWeightedScore(request.CriticalityScore);

        await _uow.SaveChangesAsync(cancellationToken);

        // Re-fetch for the response DTO — EF's identity map resolves this to the same
        // already-mutated Project/ProjectTask instances above, so this reflects the saved
        // state without a redundant round of recalculation.
        var updated = await _uow.Projects.GetWithChildrenAsync(request.ProjectId, cancellationToken);
        return updated?.ToDto();
    }
}
