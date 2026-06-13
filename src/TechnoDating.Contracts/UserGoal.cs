using System.Text.Json.Serialization;

namespace TechnoDating.Contracts;

[JsonConverter(typeof(JsonStringEnumConverter<UserGoal>))]
public enum UserGoal
{
    Friends = 1,
    Romance = 2,
    Both = 3,
}
