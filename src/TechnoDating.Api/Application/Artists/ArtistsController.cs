using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechnoDating.Api.Application.Artists.Requests;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Artists;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ArtistsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<ArtistDto>> Get(CancellationToken cancellationToken)
    {
        return mediator.Send(new GetArtistsRequest(), cancellationToken);
    }
}
