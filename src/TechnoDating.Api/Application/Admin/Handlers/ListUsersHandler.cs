using MediatR;
using Microsoft.EntityFrameworkCore;
using TechnoDating.Api.Application.Admin.Requests;
using TechnoDating.Api.Infrastructure;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Admin.Handlers;

public class ListUsersHandler(TechnoDatingDbContext db) : IRequestHandler<ListUsersRequest, IReadOnlyList<AdminUserDto>>
{
    public async Task<IReadOnlyList<AdminUserDto>> Handle(ListUsersRequest request, CancellationToken cancellationToken)
    {
        // IsProfileComplete is a [NotMapped] computed property, so pull the underlying fields
        // and compute it in memory.
        var users = await db.Users
            .AsNoTracking()
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new { u.Id, u.PhoneNumber, u.DisplayName, u.DateOfBirth, u.Gender, u.City, u.IsVerified, u.CreatedAt })
            .ToListAsync(cancellationToken);

        return users.Select(u => new AdminUserDto(
            u.Id,
            u.PhoneNumber,
            u.DisplayName,
            u.City,
            u.IsVerified,
            IsProfileComplete: !string.IsNullOrWhiteSpace(u.DisplayName)
                && u.DateOfBirth.HasValue
                && !string.IsNullOrWhiteSpace(u.Gender)
                && !string.IsNullOrWhiteSpace(u.City),
            u.CreatedAt)).ToList();
    }
}
