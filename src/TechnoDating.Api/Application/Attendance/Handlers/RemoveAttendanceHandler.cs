using MediatR;
using Microsoft.EntityFrameworkCore;
using TechnoDating.Api.Application.Attendance.Requests;
using TechnoDating.Api.Infrastructure;

namespace TechnoDating.Api.Application.Attendance.Handlers;

public class RemoveAttendanceHandler(TechnoDatingDbContext db) : IRequestHandler<RemoveAttendanceRequest, bool>
{
    public async Task<bool> Handle(RemoveAttendanceRequest request, CancellationToken cancellationToken)
    {
        var existing = await db.Attendances
            .FirstOrDefaultAsync(a => a.UserId == request.UserId && a.FestivalId == request.FestivalId, cancellationToken);
        if (existing is null)
        {
            return false;
        }

        db.Attendances.Remove(existing);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
