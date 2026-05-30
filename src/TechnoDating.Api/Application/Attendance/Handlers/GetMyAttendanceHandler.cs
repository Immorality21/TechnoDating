using MediatR;
using Microsoft.EntityFrameworkCore;
using TechnoDating.Api.Application.Attendance.Requests;
using TechnoDating.Api.Infrastructure;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Attendance.Handlers;

public class GetMyAttendanceHandler(TechnoDatingDbContext db) : IRequestHandler<GetMyAttendanceRequest, IReadOnlyList<FestivalAttendanceDto>>
{
    public async Task<IReadOnlyList<FestivalAttendanceDto>> Handle(GetMyAttendanceRequest request, CancellationToken cancellationToken)
    {
        var rows = await db.Attendances
            .AsNoTracking()
            .Where(a => a.UserId == request.UserId)
            .Join(
                db.Festivals,
                a => a.FestivalId,
                f => f.Id,
                (a, f) => new { f.Id, f.Name, f.Date, a.Status })
            .OrderBy(x => x.Date)
            .ToListAsync(cancellationToken);

        return rows
            .Select(x => new FestivalAttendanceDto(x.Id, x.Name, x.Date, x.Status))
            .ToList();
    }
}
