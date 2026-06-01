using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechnoDating.Api.Application.Discovery.Requests;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Discovery;

/// <summary>
/// The candidate feed — people you could connect with, ranked by shared festivals + distance.
/// This is discovery, not confirmed matches (those live on <c>/api/matches</c>).
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class DiscoveryController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MatchProfileDto>>> Get(CancellationToken cancellationToken)
    {
        var raw = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(raw, out var userId))
        {
            return Unauthorized();
        }
        var candidates = await mediator.Send(new GetDiscoveryRequest(userId), cancellationToken);
        return Ok(candidates);
    }
}
