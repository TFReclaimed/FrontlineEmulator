using Frontline.Features.Session.Inventory.GetInventory;

namespace Frontline.Features.Session.Inventory.Dropships.GetDropships;

public class DropshipInfo
{
    public int Index { get; set; }
    public InventoryCard[] SlottedCards { get; set; }
    public int InstanceId { get; set; }
}