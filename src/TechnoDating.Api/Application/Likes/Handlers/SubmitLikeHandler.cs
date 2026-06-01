using MediatR;
using Microsoft.EntityFrameworkCore;
using TechnoDating.Api.Application.Likes.Requests;
using TechnoDating.Api.Application.Matches;
using TechnoDating.Api.Infrastructure;
using TechnoDating.Api.Infrastructure.Entities;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Likes.Handlers;

public class SubmitLikeHandler(TechnoDatingDbContext db, IMatchmaker matchmaker) : IRequestHandler<SubmitLikeRequest, LikeResultDto?>
{
    public async Task<LikeResultDto?> Handle(SubmitLikeRequest request, CancellationToken cancellationToken)
    {
        var targetExists = await db.Users.AnyAsync(u => u.Id == request.TargetUserId, cancellationToken);
        if (!targetExists)
        {
            return null;
        }

        // Upsert the directional signal. Re-liking after a pass (or vice versa) just flips Kind.
        var existing = await db.Likes
            .FirstOrDefaultAsync(l => l.LikerId == request.LikerId && l.LikedId == request.TargetUserId, cancellationToken);
        if (existing is null)
        {
            db.Likes.Add(new Like
            {
                Id = Guid.NewGuid(),
                LikerId = request.LikerId,
                LikedId = request.TargetUserId,
                Kind = request.Kind,
                CreatedAt = DateTimeOffset.UtcNow,
            });
        }
        else
        {
            existing.Kind = request.Kind;
            existing.CreatedAt = DateTimeOffset.UtcNow;
        }

        // --- The entire "mutual like ⇒ match" policy lives in this block. ---
        // To change how matches are created (curated, algorithmic, etc.), replace or
        // disable this; nothing downstream (Match, matches list, chat) depends on it.
        if (request.Kind == LikeKind.Like)
        {
            var reciprocated = await db.Likes.AnyAsync(
                l => l.LikerId == request.TargetUserId && l.LikedId == request.LikerId && l.Kind == LikeKind.Like,
                cancellationToken);
            if (reciprocated)
            {
                // Matchmaker.SaveChangesAsync also persists the like added above.
                var match = await matchmaker.TryCreateMatchAsync(
                    request.LikerId, request.TargetUserId, MatchOrigin.MutualLike, cancellationToken);
                return new LikeResultDto(Matched: true, MatchId: match.Id);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return new LikeResultDto(Matched: false, MatchId: null);
    }
}
