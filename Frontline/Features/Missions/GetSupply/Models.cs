using System.Text.Json.Serialization;

namespace Frontline.Features.Missions.GetSupply;

public class GetSupplyResponse
{
    [JsonPropertyName("LastSupplySync")]
    public string LastSupplySync { get; set; } = string.Empty;
}