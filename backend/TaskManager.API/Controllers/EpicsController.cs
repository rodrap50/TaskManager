using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Common.DTOs;
using TaskManager.Application.Epics.Commands;
using TaskManager.Application.Epics.Queries;

namespace TaskManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EpicsController : ControllerBase
{
    private readonly IMediator _mediator;

    public EpicsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("project/{projectId:guid}")]
    public async Task<ActionResult<List<EpicDto>>> GetByProject(Guid projectId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetEpicsByProjectQuery(projectId), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<EpicDto>> CreateEpic(
        [FromBody] CreateEpicCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result is null ? NotFound() : Created(string.Empty, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<EpicDto>> UpdateEpic(
        Guid id,
        [FromBody] UpdateEpicRequest request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new UpdateEpicCommand(id, request.Name, request.Description, request.DisplayOrder), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteEpic(Guid id, CancellationToken ct)
    {
        var deleted = await _mediator.Send(new DeleteEpicCommand(id), ct);
        return deleted ? NoContent() : NotFound();
    }
}

public record UpdateEpicRequest(string? Name, string? Description, int? DisplayOrder);
