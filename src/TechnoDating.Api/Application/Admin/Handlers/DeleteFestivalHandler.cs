using MediatR;
using Microsoft.EntityFrameworkCore;
using TechnoDating.Api.Application.Admin.Requests;
using TechnoDating.Api.Infrastructure;

namespace TechnoDating.Api.Application.Admin.Handlers;

public class DeleteFestivalHandler(TechnoDatingDbContext db) : IRequestHandler<DeleteFestivalRequest, bool>
{
    public async Task<bool> Handle(DeleteFestivalRequest request, CancellationToken cancellationToken)
    {
        var festival = await db.Festivals.FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);
        if (festival is null)
        {
            return false;
        }

        // FK cascade removes the festival's headliner links and attendance rows.
        db.Festivals.Remove(festival);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
