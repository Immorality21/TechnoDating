using System.Text.Json.Serialization;

namespace TechnoDating.Contracts;

/// <summary>
/// How a <c>Match</c> came into existence. Recorded on every match so a future
/// mixed model (some mutual-like, some curated/algorithmic) coexists in one table
/// and stays analyzable. See docs/MATCHING.md.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<MatchOrigin>))]
public enum MatchOrigin
{
    MutualLike = 1,
    Curated = 2,
    Admin = 3,
    AutoFestival = 4,
}
