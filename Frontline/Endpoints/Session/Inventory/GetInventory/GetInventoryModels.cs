namespace Frontline.Endpoints.Session.Inventory.GetInventory;

public class InventoryListResponse
{
    public required List<CardDto> Items { get; set; }
}