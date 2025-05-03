using System.Text.Json.Serialization;
using Frontline.Features.Session.Inventory.GetInventory;

namespace Frontline.Game;

public class GameCard : InventoryCard
{
    public ActiveCardData? ActiveData { get; set; }
}

[JsonDerivedType(typeof(ActiveEntityCardData), "ActiveEntityCardData")]
[JsonDerivedType(typeof(ActiveUnitCardData), "ActiveUnitCardData")]
public class ActiveCardData
{
    public sbyte Owner { get; set; }
}

[JsonDerivedType(typeof(ActiveEntityCardData), "ActiveEntityCardData")]
public class ActiveEntityCardData : ActiveCardData
{
    public sbyte CurrentHealth { get; set; } = 10; // TODO
    public sbyte Acted { get; set; } = 0;
}

public class ActiveUnitCardData : ActiveEntityCardData
{
    public sbyte CurrentDefense { get; set; } = 5; // TODO
}