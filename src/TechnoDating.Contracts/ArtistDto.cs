namespace TechnoDating.Contracts;

public record ArtistRefDto(Guid Id, string Name);

public record ArtistDto(Guid Id, string Name, string? Genre);
