using MediatR;
using Microsoft.EntityFrameworkCore;
using TechnoDating.Api.Application.Attendance.Requests;
using TechnoDating.Api.Infrastructure;
using TechnoDating.Api.Infrastructure.Entities;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Attendance.Handlers;

public class UpsertAttendanceHandler(TechnoDatingDbContext db) : IRequestHandler<UpsertAttendanceRequest, FestivalAttendanceDto?>
{
    public async Task<FestivalAttendanceDto?> Handle(UpsertAttendanceRequest request, CancellationToken cancellationToken)
    {
        var festival = await db.Festivals
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == request.FestivalId, cancellationToken);
        if (festival is null)
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        var existing = await db.Attendances
            .FirstOrDefaultAsync(a => a.UserId == request.UserId && a.FestivalId == request.FestivalId, cancellationToken);

        if (existing is null)
        {
            existing = new UserFestivalAttendance
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                FestivalId = request.FestivalId,
                Status = request.Status,
                CreatedAt = now,
                UpdatedAt = now,
            };
            db.Attendances.Add(existing);
        }
        else
        {
            existing.Status = request.Status;
            existing.UpdatedAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);
        return new FestivalAttendanceDto(festival.Id, festival.Name, festival.Date, existing.Status);
    }
}
