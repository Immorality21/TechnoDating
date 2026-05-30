using MediatR;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Attendance.Requests;

public record GetMyAttendanceRequest(Guid UserId) : IRequest<IReadOnlyList<FestivalAttendanceDto>>;
