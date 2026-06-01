using System.Text.Json.Serialization;

namespace TechnoDating.Contracts;

[JsonConverter(typeof(JsonStringEnumConverter<MatchStatus>))]
public enum MatchStatus
{
    Active = 1,
    Closed = 2,
}
