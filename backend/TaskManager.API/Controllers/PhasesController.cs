using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Common.DTOs;
using TaskManager.Application.Phases.Commands;
using TaskManager.Application.Phases.Queries;

namespace TaskManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PhasesController : ControllerBase
{
    private readonly IMediator _mediator;

    public PhasesController(IMediator mediator) => _mediator = mediator;

    [HttpGet("project/{projectId:guid}")]
    public async Task<ActionResult<List<ProjectPhaseDto>>> GetByProject(Guid projectId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPhasesByProjectQuery(projectId), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ProjectPhaseDto>> CreatePhase(
        [FromBody] CreatePhaseCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result is null ? NotFound() : Created(string.Empty, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProjectPhaseDto>> UpdatePhase(
        Guid id,
        [FromBody] UpdatePhaseRequest request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new UpdatePhaseCommand(id, request.Name, request.Description, request.DisplayOrder), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeletePhase(Guid id, CancellationToken ct)
    {
        var deleted = await _mediator.Send(new DeletePhaseCommand(id), ct);
        return deleted ? NoContent() : NotFound();
    }
}

public record UpdatePhaseRequest(string? Name, string? Description, int? DisplayOrder);
