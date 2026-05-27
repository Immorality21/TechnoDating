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
    public Task<IReadOnlyList<FestivalDto>> Get(CancellationToken cancellationToken)
    {
        return mediator.Send(new GetFestivalsRequest(), cancellationToken);
    }
}
