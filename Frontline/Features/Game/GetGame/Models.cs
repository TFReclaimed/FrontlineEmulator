using System.Text.Json.Serialization;
using Frontline.Features.Session.Inventory.GetInventory;
using Frontline.Features.Session.Rulesets;
using Frontline.Game;

namespace Frontline.Features.Game.GetGame;

public class GetGameRequest
{
    public Guid GameId { get; set; }
}

public class GetGameResponse
{
    public required GameState GameState { get; set; }
    public required RulesetPathResponse RulesetPath { get; set; }
    public int GameChangeCounter { get; set; }
    public int CurrentEventCount { get; set; }
}

public class GameState
{
    public Guid GameInstanceId { get; set; }
    public required List<Player> Players { get; set; }
    public required GameBoard Board { get; set; }
    public int GameTemplateId { get; set; }
    public sbyte CurrentRound { get; set; }
    public sbyte PlayerTurn { get; set; }
    public long PlayerTurnStart { get; set; }
    public long PlayerDiscardStart { get; set; }
    public sbyte LocalPlayer { get; set; }
    public sbyte WinningPlayer { get; set; }
    public bool SurrenderGameOver { get; set; }
    public required Rewards[] Rewards { get; set; }
    public int NextSummonInstanceId { get; set; }
    public VersusType GameType { get; set; }
}

public class Player
{
    public required Deck Deck { get; set; }
    public required SupportDeck SupportDeck { get; set; }
    public required CardCollection Hand { get; set; }
    public required CardCollection Discard { get; set; }
    public required GameResources Resources { get; set; }
    public required CardStack Commander { get; set; }
    public string Name { get; set; } = string.Empty;
    public int UserId { get; set; }
}

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

public class GameResources
{
    public sbyte CommandAccum { get; set; }
    public sbyte CommandUnits { get; set; }
    public sbyte Health { get; set; }
    public sbyte MaxHealth { get; set; }
    public sbyte DrawDamage { get; set; }
}

public class GameBoard
{
    public required List<GameRegion> Regions { get; set; }
}

public class GameRegion
{
    public required List<CardStack> Slots { get; set; }
    public RegionEnum RegionLocation { get; set; }
}

public class CardStack
{
    public InventoryCard? PrimaryCard { get; set; }
}

public class Rewards
{
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RegionEnum
{
    Player0 = 0,
    Player1 = 1,
    Control = 2
}