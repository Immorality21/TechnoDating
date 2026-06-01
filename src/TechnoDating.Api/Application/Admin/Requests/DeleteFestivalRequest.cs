using MediatR;

namespace TechnoDating.Api.Application.Admin.Requests;

/// <summary>Returns false when the festival doesn't exist.</summary>
public record DeleteFestivalRequest(Guid Id) : IRequest<bool>;
