using MediatR;
using Microsoft.AspNetCore.Mvc;
using TechnoDating.Api.Application.Admin.Requests;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Admin;

[ApiController]
[ServiceFilter(typeof(AdminApiKeyFilter))]
[Route("api/admin/users")]
public class AdminUsersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminUserDto>>> Get(CancellationToken cancellationToken)
    {
        var users = await mediator.Send(new ListUsersRequest(), cancellationToken);
        return Ok(users);
    }
}
