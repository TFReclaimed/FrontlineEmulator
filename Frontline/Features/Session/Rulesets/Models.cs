using System.Text.Json.Serialization;

namespace Frontline.Features.Session.Rulesets;

public class RulesetPathResponse
{
    public string Uri { get; set; }
    public int Version { get; set; }
    [JsonPropertyName("cardsuri")]
    public string CardsUri { get; set; }
    [JsonPropertyName("gamesuri")]
    public string GamesUri { get; set; }
    [JsonPropertyName("fusionsuri")]
    public string FusionsUri { get; set; }
    [JsonPropertyName("dropshipssuri")]
    public string DropshipsUri { get; set; }
}