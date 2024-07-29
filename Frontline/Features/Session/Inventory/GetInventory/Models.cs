using System.Text.Json.Serialization;
using Frontline.Missions;
using Frontline.Utils;

namespace Frontline.Features.Session.Inventory.GetInventory;

public class InventoryListResponse
{
    [JsonPropertyName("$types")]
    public Dictionary<string, string> Types { get; set; } = new()
    {
        ["Card"] = "1"
    };
    public List<InventoryCard> Items { get; set; }
}

public class InventoryCard
{
    [JsonPropertyName("$type")]
    public string Type { get; set; } = "1";
    public int InstanceId { get; set; }
    public int TemplateId { get; set; }
    [JsonConverter(typeof(JsonStringConverter<CardData>))]
    public CardData? GameData { get; set; }
    public int Xp { get; set; }
    public sbyte Rank { get; set; }
}

public class CardData
{
    [JsonConverter(typeof(JsonStringConverter<CardAvailability>))]
    public CardAvailability? Availability { get; set; }
}

public class CardAvailability
{
    [JsonPropertyName("PvECardState")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CardState CardState { get; set; }
    [JsonPropertyName("pve_region")]
    public PveRegion Region { get; set; }
    [JsonPropertyName("pve_faction")]
    public Faction Faction { get; set; }
    [JsonPropertyName("pve_missionid")]
    public int MissionId { get; set; }
}

public enum CardState
{
    None = 0,
    OnMission = 1,
    InDropship = 2,
    Casualty = 3
}