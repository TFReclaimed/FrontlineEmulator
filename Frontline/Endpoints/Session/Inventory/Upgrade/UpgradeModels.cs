using Frontline.Data.Entities;

namespace Frontline.Endpoints.Session.Inventory.Upgrade;

public class UpgradeRequest
{
    public int ItemId { get; set; }
}

// The game only cares about the rank of the upgraded card
public class UpgradedCard
{
    public int Rank { get; set; }

    public static UpgradedCard FromEntity(ItemEntity entity)
    {
        return new UpgradedCard
        {
            Rank = entity.Rank
        };
    }
}