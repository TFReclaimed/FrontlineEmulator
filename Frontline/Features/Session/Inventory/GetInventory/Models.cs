using Frontline.Data.Entities;

namespace Frontline.Features.Session.Inventory.GetInventory;

public class InventoryListResponse
{
    public required List<CardDto> Items { get; set; }
}