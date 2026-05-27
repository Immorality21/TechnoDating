using MediatR;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Auth.Requests;

public record VerifyOtpRequest(string PhoneNumber, string Code) : IRequest<AuthResponseDto?>;
