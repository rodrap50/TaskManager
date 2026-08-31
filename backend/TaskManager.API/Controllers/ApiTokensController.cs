using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.ApiTokens.Commands;
using TaskManager.Application.ApiTokens.Queries;
using TaskManager.Application.Common.DTOs;
using TaskManager.Application.Common.Interfaces;

namespace TaskManager.API.Controllers;

[ApiController]
[Route("api/api-tokens")]
[Authorize(Roles = "Admin")]
public class ApiTokensController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public ApiTokensController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator    = mediator;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<List<ApiTokenDto>>> GetTokens(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetApiTokensQuery(), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CreateApiTokenResult>> CreateToken(
        [FromBody] CreateApiTokenRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateApiTokenCommand(request.Name, _currentUser.UserId), ct);
        return Created(string.Empty, result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> RevokeToken(Guid id, CancellationToken ct)
    {
        var revoked = await _mediator.Send(new RevokeApiTokenCommand(id), ct);
        return revoked ? NoContent() : NotFound();
    }
}

public record CreateApiTokenRequest(string Name);
