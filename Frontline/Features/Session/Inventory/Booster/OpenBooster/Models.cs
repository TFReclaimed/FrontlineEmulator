namespace Frontline.Features.Session.Inventory.Booster.OpenBooster;

public class OpenBoosterPackRequest
{
    public int BoosterId { get; set; }
}

public class BoosterPackResponse
{
    public List<object> Cards { get; set; }
    public List<object> Resources { get; set; }
}