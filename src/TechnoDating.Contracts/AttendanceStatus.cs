using System.Text.Json.Serialization;

namespace TechnoDating.Contracts;

[JsonConverter(typeof(JsonStringEnumConverter<AttendanceStatus>))]
public enum AttendanceStatus
{
    Interested = 1,
    Going = 2,
    Ticketed = 3,
}
