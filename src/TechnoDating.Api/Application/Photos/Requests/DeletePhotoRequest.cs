using MediatR;

namespace TechnoDating.Api.Application.Photos.Requests;

public record DeletePhotoRequest(Guid UserId, Guid PhotoId) : IRequest<bool>;
