using MediatR;
using Microsoft.AspNetCore.Mvc;
using TechnoDating.Api.Application.Auth.Requests;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Auth;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("request-otp")]
    public async Task<IActionResult> RequestOtp([FromBody] RequestOtpDto body, CancellationToken cancellationToken)
    {
        var accepted = await mediator.Send(new RequestOtpRequest(body.PhoneNumber), cancellationToken);
        if (!accepted)
        {
            return BadRequest(new { error = "invalid_phone_or_cooldown" });
        }
        return Ok();
    }

    [HttpPost("verify-otp")]
    public async Task<ActionResult<AuthResponseDto>> VerifyOtp([FromBody] VerifyOtpDto body, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new VerifyOtpRequest(body.PhoneNumber, body.Code), cancellationToken);
        if (result is null)
        {
            return Unauthorized(new { error = "otp_invalid" });
        }
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> Refresh([FromBody] RefreshTokenDto body, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new RefreshTokenRequest(body.RefreshToken), cancellationToken);
        if (result is null)
        {
            return Unauthorized(new { error = "refresh_invalid" });
        }
        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenDto body, CancellationToken cancellationToken)
    {
        await mediator.Send(new LogoutRequest(body.RefreshToken), cancellationToken);
        return NoContent();
    }
}
