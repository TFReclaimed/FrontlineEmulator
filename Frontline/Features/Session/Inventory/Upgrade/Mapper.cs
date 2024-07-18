using FastEndpoints;
using Frontline.Data.Entities;

namespace Frontline.Features.Session.Inventory.Upgrade;

public class Mapper : Mapper<UpgradeRequest, UpgradedCard, ItemEntity>
{
    public override UpgradedCard FromEntity(ItemEntity e)
    {
        return new UpgradedCard
        {
            Rank = e.Rank
        };
    }
}