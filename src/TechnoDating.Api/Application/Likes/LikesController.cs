using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechnoDating.Api.Application.Likes.Requests;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Likes;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class LikesController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<LikeResultDto>> Submit([FromBody] SubmitLikeDto body, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        if (body.TargetUserId == userId)
        {
            return BadRequest(new { error = "cannot_like_self" });
        }
        var result = await mediator.Send(new SubmitLikeRequest(userId, body.TargetUserId, body.Kind), cancellationToken);
        if (result is null)
        {
            return NotFound(new { error = "user_not_found" });
        }
        return Ok(result);
    }

    private bool TryGetUserId(out Guid userId)
    {
        var raw = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(raw, out userId);
    }
}
