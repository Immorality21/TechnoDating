using MediatR;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Photos.Requests;

public record SetPrimaryPhotoRequest(Guid UserId, Guid PhotoId) : IRequest<PhotoDto?>;
