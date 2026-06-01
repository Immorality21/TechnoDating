using MediatR;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Likes.Requests;

/// <summary>Returns null when the target user does not exist.</summary>
public record SubmitLikeRequest(Guid LikerId, Guid TargetUserId, LikeKind Kind) : IRequest<LikeResultDto?>;
