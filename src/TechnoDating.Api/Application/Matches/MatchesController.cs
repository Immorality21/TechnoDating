using MediatR;
using Microsoft.AspNetCore.Mvc;
using TechnoDating.Api.Application.Matches.Requests;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Matches;

[ApiController]
[Route("api/[controller]")]
public class MatchesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<MatchProfileDto>> Get(CancellationToken cancellationToken)
    {
        return mediator.Send(new GetMatchesRequest(), cancellationToken);
    }
}
