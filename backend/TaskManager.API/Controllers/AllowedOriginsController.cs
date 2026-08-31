using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.AllowedOrigins.Commands;
using TaskManager.Application.AllowedOrigins.Queries;
using TaskManager.Application.Common.DTOs;

namespace TaskManager.API.Controllers;

[ApiController]
[Route("api/allowed-origins")]
[Authorize(Roles = "Admin")]
public class AllowedOriginsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AllowedOriginsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<List<AllowedOriginDto>>> GetAllowedOrigins(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllowedOriginsQuery(), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<AllowedOriginDto>> AddAllowedOrigin(
        [FromBody] AddAllowedOriginRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new AddAllowedOriginCommand(request.OriginUrl), ct);
        return Created(string.Empty, result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> RemoveAllowedOrigin(Guid id, CancellationToken ct)
    {
        var removed = await _mediator.Send(new RemoveAllowedOriginCommand(id), ct);
        return removed ? NoContent() : NotFound();
    }
}

public record AddAllowedOriginRequest(string OriginUrl);
