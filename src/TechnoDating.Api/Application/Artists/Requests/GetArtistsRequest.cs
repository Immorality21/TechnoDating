using MediatR;
using TechnoDating.Contracts;

namespace TechnoDating.Api.Application.Artists.Requests;

public record GetArtistsRequest() : IRequest<IReadOnlyList<ArtistDto>>;
