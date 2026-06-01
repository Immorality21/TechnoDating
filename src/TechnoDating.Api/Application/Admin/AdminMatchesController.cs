using MediatR;
using Microsoft.AspNetCore.Mvc;
using TechnoDating.Api.Application.Admin.Requests;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Admin;

[ApiController]
[ServiceFilter(typeof(AdminApiKeyFilter))]
[Route("api/admin/matches")]
public class AdminMatchesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminMatchDto>>> Get(CancellationToken cancellationToken)
    {
        var matches = await mediator.Send(new ListMatchesRequest(), cancellationToken);
        return Ok(matches);
    }

    [HttpPost("force")]
    public async Task<ActionResult<AdminMatchDto>> Force([FromBody] ForceMatchDto body, CancellationToken cancellationToken)
    {
        if (body.UserAId == body.UserBId)
        {
            return BadRequest(new { error = "same_user" });
        }
        var match = await mediator.Send(new ForceMatchRequest(body.UserAId, body.UserBId), cancellationToken);
        if (match is null)
        {
            return NotFound(new { error = "user_not_found" });
        }
        return Ok(match);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Close(Guid id, CancellationToken cancellationToken)
    {
        var closed = await mediator.Send(new CloseMatchRequest(id), cancellationToken);
        if (!closed)
        {
            return NotFound(new { error = "match_not_found" });
        }
        return NoContent();
    }
}
