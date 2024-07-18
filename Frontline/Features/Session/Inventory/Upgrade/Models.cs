namespace Frontline.Features.Session.Inventory.Upgrade;

public class UpgradeRequest
{
    public int ItemId { get; set; }
}

// The game only cares about the rank of the upgraded card
public class UpgradedCard
{
    public int Rank { get; set; }
}