using MediatR;

namespace TechnoDating.Api.Application.Admin.Requests;

/// <summary>Returns false when the match doesn't exist.</summary>
public record CloseMatchRequest(Guid MatchId) : IRequest<bool>;
