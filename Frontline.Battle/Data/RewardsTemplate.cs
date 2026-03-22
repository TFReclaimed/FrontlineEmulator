using System.Text.Json.Serialization;

namespace Frontline.Battle.Data;

public class RewardsTemplate
{
    public string RewardGroup { get; set; } = string.Empty;
    [JsonPropertyName("playerXP")]
    public int PlayerXp { get; set; }
    public int Trophies { get; set; }
    public int Credits { get; set; }
    public int Supply { get; set; }
    public int Boosters { get; set; }
    public int Tokens { get; set; }
}