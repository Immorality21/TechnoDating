using TechnoDating.Api.Infrastructure.Entities;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Users;

public static class UserMappingExtensions
{
    public static UserProfileDto ToProfileDto(this User user)
    {
        return new UserProfileDto(
            user.Id,
            user.PhoneNumber ?? string.Empty,
            user.DisplayName,
            user.DateOfBirth,
            user.Gender,
            user.Bio,
            user.City,
            user.TopArtists,
            user.IsVerified,
            user.IsProfileComplete);
    }
}
