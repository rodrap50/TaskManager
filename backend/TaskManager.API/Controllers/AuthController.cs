using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Auth.Commands;
using TaskManager.Application.Auth.Queries;

namespace TaskManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResult>> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result is null ? Unauthorized() : Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("setup/status")]
    public async Task<ActionResult<SetupStatusDto>> GetSetupStatus(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSetupStatusQuery(), ct);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("setup")]
    public async Task<ActionResult<LoginResult>> Setup([FromBody] SetupAdminCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return result is null ? Conflict() : Ok(result);
    }
}
