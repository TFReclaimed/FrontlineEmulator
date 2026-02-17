using System.Text.Json.Serialization;

namespace Frontline.Endpoints.Missions.GetMissions;

public class GetMissionsResponse
{
    [JsonPropertyName("Version")]
    public string Version { get; set; } = string.Empty;
    [JsonPropertyName("Data")]
    public string Data { get; set; } = string.Empty;
}