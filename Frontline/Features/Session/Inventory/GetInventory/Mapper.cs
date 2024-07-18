using FastEndpoints;
using Frontline.Data.Entities;

namespace Frontline.Features.Session.Inventory.GetInventory;

public class Mapper : Mapper<GetInventoryRequest, List<InventoryCard>, List<ItemEntity>>
{
    public override List<InventoryCard> FromEntity(List<ItemEntity> e)
    {
        return e.Select(item => new InventoryCard
        {
            InstanceId = item.ItemId,
            TemplateId = item.TemplateId,
            Xp = item.Xp,
            Rank = item.Rank
        }).ToList();
    }
}