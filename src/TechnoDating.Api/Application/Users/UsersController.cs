using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechnoDating.Api.Application.Users.Requests;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Users;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UsersController(IMediator mediator) : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<UserProfileDto>> GetMe(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var profile = await mediator.Send(new GetMeRequest(userId), cancellationToken);
        if (profile is null)
        {
            return NotFound();
        }
        return Ok(profile);
    }

    [HttpPut("me")]
    public async Task<ActionResult<UserProfileDto>> UpdateMe([FromBody] UpdateProfileDto body, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var profile = await mediator.Send(new UpdateMeRequest(userId, body), cancellationToken);
        if (profile is null)
        {
            return NotFound();
        }
        return Ok(profile);
    }

    private bool TryGetUserId(out Guid userId)
    {
        var raw = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(raw, out userId);
    }
}
