using System.Text.RegularExpressions;
using MediatR;
using TechnoDating.Api.Application.Auth.Requests;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Auth.Handlers;

public partial class VerifyOtpHandler(IOtpService otp, ITokenService tokens) : IRequestHandler<VerifyOtpRequest, AuthResponseDto?>
{
    public async Task<AuthResponseDto?> Handle(VerifyOtpRequest request, CancellationToken cancellationToken)
    {
        if (!E164().IsMatch(request.PhoneNumber))
        {
            return null;
        }
        if (string.IsNullOrWhiteSpace(request.Code))
        {
            return null;
        }

        var user = await otp.VerifyAsync(request.PhoneNumber, request.Code, cancellationToken);
        if (user is null)
        {
            return null;
        }

        return await tokens.IssueAsync(user, cancellationToken);
    }

    [GeneratedRegex(@"^\+[1-9]\d{1,14}$")]
    private static partial Regex E164();
}
