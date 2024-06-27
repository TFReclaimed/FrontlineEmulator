using System.Text.Json.Serialization;
using Frontline.Game;

namespace Frontline.Features.Session.Inventory.GetInventory;

public class InventoryListResponse
{
    [JsonPropertyName("$types")]
    public Dictionary<string, string> Types { get; set; } = new()
    {
        ["CommanderCard"] = "1"
    };
    public Item[] Items { get; set; }
}