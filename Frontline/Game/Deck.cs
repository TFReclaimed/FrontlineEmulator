using Frontline.Features.Session.Inventory.GetInventory;

namespace Frontline.Game;

public class Deck
{
    public required List<InventoryCard> Cards { get; set; }
    public sbyte Count { get; set; }
}

public class SupportDeck : Deck
{
    public sbyte CurrentSupport { get; set; }
    public InventoryCard? Repeater { get; set; }
    public InventoryCard? Ultimate { get; set; }
    public bool CanRepeat { get; set; }
    public bool NoShuffle { get; set; }
}

public class CardCollection
{
    public required List<InventoryCard> Cards { get; set; }
}

public class CardStack
{
    public InventoryCard? PrimaryCard { get; set; }
}