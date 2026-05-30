using MediatR;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Attendance.Requests;

public record UpsertAttendanceRequest(Guid UserId, Guid FestivalId, AttendanceStatus Status) : IRequest<FestivalAttendanceDto?>;
