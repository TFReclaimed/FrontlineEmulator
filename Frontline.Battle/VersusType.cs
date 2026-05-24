using System.Text.Json.Serialization;

namespace Frontline.Battle;

[JsonConverter(typeof(JsonStringEnumConverter<VersusType>))]
public enum VersusType
{
    [JsonStringEnumMemberName("PVP_RANKED")]
    PvpRanked,
    [JsonStringEnumMemberName("PVP_CASUAL")]
    PvpCasual,
    [JsonStringEnumMemberName("PVP_AIREMOTE")]
    PvpAiRemote
}