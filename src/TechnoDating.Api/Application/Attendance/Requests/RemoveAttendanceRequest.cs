using MediatR;

namespace TechnoDating.Api.Application.Attendance.Requests;

public record RemoveAttendanceRequest(Guid UserId, Guid FestivalId) : IRequest<bool>;
