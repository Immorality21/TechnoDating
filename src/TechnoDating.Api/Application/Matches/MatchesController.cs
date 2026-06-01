using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechnoDating.Api.Application.Matches.Requests;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Matches;

/// <summary>Confirmed mutual matches. The candidate feed lives on <c>/api/discovery</c>.</summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class MatchesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MatchDto>>> Get(CancellationToken cancellationToken)
    {
        var raw = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(raw, out var userId))
        {
            return Unauthorized();
        }
        var matches = await mediator.Send(new GetMatchesRequest(userId), cancellationToken);
        return Ok(matches);
    }
}
