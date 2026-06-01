using MediatR;
using Microsoft.EntityFrameworkCore;
using TechnoDating.Api.Application.Matches.Requests;
using TechnoDating.Api.Application.Photos;
using TechnoDating.Api.Application.Storage;
using TechnoDating.Api.Infrastructure;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Matches.Handlers;

/// <summary>
/// Confirmed mutual connections for the current user. Reads <c>Match</c> only — it is
/// deliberately agnostic to how each match was created (mutual-like, curated, etc.).
/// </summary>
public class GetMatchesHandler(TechnoDatingDbContext db, IBlobStorage storage) : IRequestHandler<GetMatchesRequest, IReadOnlyList<MatchDto>>
{
    public async Task<IReadOnlyList<MatchDto>> Handle(GetMatchesRequest request, CancellationToken cancellationToken)
    {
        var me = request.CurrentUserId;

        var matches = await db.Matches
            .AsNoTracking()
            .Where(m => m.Status == MatchStatus.Active && (m.UserAId == me || m.UserBId == me))
            .Select(m => new
            {
                m.Id,
                m.CreatedAt,
                OtherUserId = m.UserAId == me ? m.UserBId : m.UserAId,
            })
            .ToListAsync(cancellationToken);

        if (matches.Count == 0)
        {
            return [];
        }

        var otherIds = matches.Select(m => m.OtherUserId).ToList();
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        var users = await db.Users
            .AsNoTracking()
            .Where(u => otherIds.Contains(u.Id)
                && u.DisplayName != null
                && u.DateOfBirth != null
                && u.City != null)
            .Select(u => new { u.Id, u.DisplayName, u.DateOfBirth, u.City })
            .ToDictionaryAsync(u => u.Id, cancellationToken);

        var primaryPhotoUrls = await db.LoadPrimaryPhotoCardUrlsAsync(storage, otherIds, cancellationToken);

        var result = matches
            .Where(m => users.ContainsKey(m.OtherUserId))
            .Select(m =>
            {
                var u = users[m.OtherUserId];
                return new MatchDto(
                    MatchId: m.Id,
                    UserId: m.OtherUserId,
                    DisplayName: u.DisplayName!,
                    Age: CalculateAge(u.DateOfBirth!.Value, today),
                    City: u.City!,
                    PrimaryPhotoUrl: primaryPhotoUrls.TryGetValue(m.OtherUserId, out var url) ? url : null,
                    MatchedAt: m.CreatedAt);
            })
            .OrderByDescending(m => m.MatchedAt)
            .ToList();

        return result;
    }

    private static int CalculateAge(DateOnly dob, DateOnly today)
    {
        var age = today.Year - dob.Year;
        if (today < dob.AddYears(age))
        {
            age--;
        }
        return age;
    }
}
