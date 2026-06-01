namespace TechnoDating.Contracts;

/// <summary>Body for <c>POST /api/likes</c> — express directional interest (or a pass).</summary>
public record SubmitLikeDto(Guid TargetUserId, LikeKind Kind);
