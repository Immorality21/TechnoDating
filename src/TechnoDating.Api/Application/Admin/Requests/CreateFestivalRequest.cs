using MediatR;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Admin.Requests;

public record CreateFestivalRequest(SaveFestivalDto Festival) : IRequest<AdminFestivalDto>;
