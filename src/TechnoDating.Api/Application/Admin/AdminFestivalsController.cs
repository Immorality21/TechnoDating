using MediatR;
using Microsoft.AspNetCore.Mvc;
using TechnoDating.Api.Application.Admin.Requests;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Admin;

[ApiController]
[ServiceFilter(typeof(AdminApiKeyFilter))]
[Route("api/admin/festivals")]
public class AdminFestivalsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminFestivalDto>>> Get(CancellationToken cancellationToken)
    {
        var festivals = await mediator.Send(new ListFestivalsRequest(), cancellationToken);
        return Ok(festivals);
    }

    [HttpPost]
    public async Task<ActionResult<AdminFestivalDto>> Create([FromBody] SaveFestivalDto body, CancellationToken cancellationToken)
    {
        var created = await mediator.Send(new CreateFestivalRequest(body), cancellationToken);
        return Ok(created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AdminFestivalDto>> Update(Guid id, [FromBody] SaveFestivalDto body, CancellationToken cancellationToken)
    {
        var updated = await mediator.Send(new UpdateFestivalRequest(id, body), cancellationToken);
        if (updated is null)
        {
            return NotFound(new { error = "festival_not_found" });
        }
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await mediator.Send(new DeleteFestivalRequest(id), cancellationToken);
        if (!deleted)
        {
            return NotFound(new { error = "festival_not_found" });
        }
        return NoContent();
    }
}
