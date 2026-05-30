using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechnoDating.Api.Application.Festivals.Requests;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Festivals;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class FestivalsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FestivalDto>>> Get(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var festivals = await mediator.Send(new GetFestivalsRequest(userId), cancellationToken);
        return Ok(festivals);
    }

    [HttpGet("{festivalId:guid}/attendees")]
    public async Task<ActionResult<IReadOnlyList<MatchProfileDto>>> GetAttendees(Guid festivalId, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var attendees = await mediator.Send(new GetFestivalAttendeesRequest(userId, festivalId), cancellationToken);
        return Ok(attendees);
    }

    private bool TryGetUserId(out Guid userId)
    {
        var raw = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(raw, out userId);
    }
}
