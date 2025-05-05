using Frontline.Features.Session.Inventory.GetInventory;

namespace Frontline.Features.Session.Inventory.Booster.OpenBooster;

public class OpenBoosterPackRequest
{
    public int BoosterId { get; set; }
}

public class BoosterPackResponse
{
    public required List<InventoryCard> Cards { get; set; }
    public required List<InventoryCard> Resources { get; set; }
}