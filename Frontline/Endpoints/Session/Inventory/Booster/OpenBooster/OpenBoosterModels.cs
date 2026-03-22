namespace Frontline.Endpoints.Session.Inventory.Booster.OpenBooster;

public class OpenBoosterPackRequest
{
    public int BoosterId { get; set; }
}

public class BoosterPackResponse
{
    public required List<CardDto> Cards { get; set; }
    public required List<CardDto> Resources { get; set; }
}