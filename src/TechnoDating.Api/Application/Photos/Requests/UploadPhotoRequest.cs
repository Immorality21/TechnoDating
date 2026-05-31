using MediatR;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Photos.Requests;

public record UploadPhotoRequest(Guid UserId, Stream Content, string ContentType) : IRequest<PhotoDto?>;
