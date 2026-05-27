using MediatR;

namespace TechnoDating.Api.Application.Auth.Requests;

public record RequestOtpRequest(string PhoneNumber) : IRequest<bool>;
