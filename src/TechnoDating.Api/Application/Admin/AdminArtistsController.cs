using MediatR;
using Microsoft.AspNetCore.Mvc;
using TechnoDating.Api.Application.Admin.Requests;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Admin;

[ApiController]
[ServiceFilter(typeof(AdminApiKeyFilter))]
[Route("api/admin/artists")]
public class AdminArtistsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ArtistDto>>> Get(CancellationToken cancellationToken)
    {
        var artists = await mediator.Send(new ListAdminArtistsRequest(), cancellationToken);
        return Ok(artists);
    }
}
