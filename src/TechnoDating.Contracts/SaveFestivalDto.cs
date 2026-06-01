namespace TechnoDating.Contracts;

/// <summary>Create/update body for a festival. Headliners are an ordered set of artist ids (billing order = list order).</summary>
public record SaveFestivalDto(
    string Name,
    DateOnly Date,
    string City,
    string Venue,
    IReadOnlyList<Guid> HeadlinerArtistIds);
