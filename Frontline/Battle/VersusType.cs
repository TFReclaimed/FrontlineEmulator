using System.Text.Json.Serialization;

namespace Frontline.Battle;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VersusType
{
    [JsonStringEnumMemberName("PVP_RANKED")]
    PvpRanked,
    [JsonStringEnumMemberName("PVP_CASUAL")]
    PvpCasual
}