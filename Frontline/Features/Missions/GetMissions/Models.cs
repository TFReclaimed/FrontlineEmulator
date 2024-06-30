using System.Text.Json.Serialization;

namespace Frontline.Features.Missions.GetMissions;

public class GetMissionsResponse
{
    [JsonPropertyName("Version")]
    public string Version { get; set; }
    [JsonPropertyName("Data")]
    public string Data { get; set; }
}