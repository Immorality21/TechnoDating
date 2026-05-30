using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechnoDating.Api.Application.Attendance.Requests;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Attendance;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AttendanceController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FestivalAttendanceDto>>> GetMine(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var rows = await mediator.Send(new GetMyAttendanceRequest(userId), cancellationToken);
        return Ok(rows);
    }

    [HttpPut("{festivalId:guid}")]
    public async Task<ActionResult<FestivalAttendanceDto>> Upsert(Guid festivalId, [FromBody] UpsertAttendanceDto body, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var result = await mediator.Send(new UpsertAttendanceRequest(userId, festivalId, body.Status), cancellationToken);
        if (result is null)
        {
            return NotFound(new { error = "festival_not_found" });
        }
        return Ok(result);
    }

    [HttpDelete("{festivalId:guid}")]
    public async Task<IActionResult> Remove(Guid festivalId, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        await mediator.Send(new RemoveAttendanceRequest(userId, festivalId), cancellationToken);
        return NoContent();
    }

    private bool TryGetUserId(out Guid userId)
    {
        var raw = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(raw, out userId);
    }
}
