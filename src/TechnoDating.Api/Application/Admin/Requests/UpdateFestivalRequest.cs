using MediatR;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Admin.Requests;

/// <summary>Returns null when the festival doesn't exist.</summary>
public record UpdateFestivalRequest(Guid Id, SaveFestivalDto Festival) : IRequest<AdminFestivalDto?>;
