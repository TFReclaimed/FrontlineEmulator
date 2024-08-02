using Frontline.Game;

namespace Frontline.Features.Session.Inventory.Booster.OpenBooster;

public class OpenBoosterPackRequest
{
    public int BoosterId { get; set; }
}

public class BoosterPackResponse
{
    public required List<Card> Cards { get; set; }
    public required List<ResourceCard> Resources { get; set; }
}