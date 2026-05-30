using TechnoDating.Contracts;

namespace TechnoDating.Api.Infrastructure.Entities;

public class UserFestivalAttendance
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid FestivalId { get; set; }
    public AttendanceStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public User? User { get; set; }
    public Festival? Festival { get; set; }
}
