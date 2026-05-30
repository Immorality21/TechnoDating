namespace TechnoDating.Contracts;

public record FestivalAttendanceDto(
    Guid FestivalId,
    string FestivalName,
    DateOnly Date,
    AttendanceStatus Status);
