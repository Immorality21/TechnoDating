using MediatR;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Admin.Requests;

/// <summary>Returns null when one or both users don't exist.</summary>
public record ForceMatchRequest(Guid UserAId, Guid UserBId) : IRequest<AdminMatchDto?>;
