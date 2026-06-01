using MediatR;
using Microsoft.EntityFrameworkCore;
using TechnoDating.Api.Application.Admin.Requests;
using TechnoDating.Api.Infrastructure;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Admin.Handlers;

public class ListMatchesHandler(TechnoDatingDbContext db) : IRequestHandler<ListMatchesRequest, IReadOnlyList<AdminMatchDto>>
{
    public async Task<IReadOnlyList<AdminMatchDto>> Handle(ListMatchesRequest request, CancellationToken cancellationToken)
    {
        return await (
            from m in db.Matches.AsNoTracking()
            join a in db.Users.AsNoTracking() on m.UserAId equals a.Id
            join b in db.Users.AsNoTracking() on m.UserBId equals b.Id
            orderby m.CreatedAt descending
            select new AdminMatchDto(
                m.Id,
                m.UserAId,
                a.DisplayName,
                m.UserBId,
                b.DisplayName,
                m.Origin,
                m.Status,
                m.CreatedAt)).ToListAsync(cancellationToken);
    }
}
