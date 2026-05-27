using System.Text.RegularExpressions;
using MediatR;
using TechnoDating.Api.Application.Auth.Requests;

namespace TechnoDating.Api.Application.Auth.Handlers;

public partial class RequestOtpHandler(IOtpService otp) : IRequestHandler<RequestOtpRequest, bool>
{
    public Task<bool> Handle(RequestOtpRequest request, CancellationToken cancellationToken)
    {
        if (!E164().IsMatch(request.PhoneNumber))
        {
            return Task.FromResult(false);
        }
        return otp.RequestAsync(request.PhoneNumber, cancellationToken);
    }

    [GeneratedRegex(@"^\+[1-9]\d{1,14}$")]
    private static partial Regex E164();
}
