using Microsoft.AspNetCore.Identity;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Infrastructure.Entities;

public class User : IdentityUser<Guid>
{
    public string? DisplayName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Bio { get; set; }
    public string? City { get; set; }
    public bool IsVerified { get; set; }
    public UserGoal Goal { get; set; } = UserGoal.Both;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset LastActiveAt { get; set; }

    public Guid? PrimaryPhotoId { get; set; }
    public Photo? PrimaryPhoto { get; set; }

    public bool IsProfileComplete =>
        !string.IsNullOrWhiteSpace(DisplayName)
        && DateOfBirth.HasValue
        && !string.IsNullOrWhiteSpace(Gender)
        && !string.IsNullOrWhiteSpace(City);
}
