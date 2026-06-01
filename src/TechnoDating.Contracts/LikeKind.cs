using System.Text.Json.Serialization;

namespace TechnoDating.Contracts;

[JsonConverter(typeof(JsonStringEnumConverter<LikeKind>))]
public enum LikeKind
{
    Like = 1,
    Pass = 2,
}
