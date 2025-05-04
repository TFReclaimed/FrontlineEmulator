using System.Text.Json.Serialization;
using Frontline.Features.Game.GetGame;

namespace Frontline.Game;

[JsonDerivedType(typeof(GameEventParams), "GameEventParams")]
[JsonDerivedType(typeof(GameEventMulliganParams), "GameEventMulliganParams")]
[JsonDerivedType(typeof(GameEventCardParams), "GameEventCardParams")]
[JsonDerivedType(typeof(GameEventRegionTarget), "GameEventRegionTarget")]
[JsonDerivedType(typeof(GameEventMessageParams), "GameEventMessageParams")]
[JsonDerivedType(typeof(GameEventEndTurnParams), "GameEventEndTurnParams")]
public class GameEventParams
{
    public sbyte PlayerIndex { get; set; } = -1;
    public GameEvent GameEvent { get; set; } = GameEvent.NumEvents;
    public GameEventResult? EventResult { get; set; }
    public List<CcgEventData>? CcgEventsLog { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GameEvent
{
    Deploy = 0,
    Attack = 1,
    Move = 2,
    ActivateTrait = 3,
    DoInitialSwap = 4,
    EndTurn = 5,
    Surrender = 6,
    Message = 7,
    DiscardCard = 8,
    Disembark = 9,
    TriggerEndTurnTraits = 10,
    Cheat_GiveCardAndCmdPts = 11,
    Interactions = 12,
    NumEvents = 13
}

public class GameEventCardParams : GameEventParams
{
    public int ActingCardId { get; set; }
}

public class GameEventRegionTarget : GameEventCardParams
{
    public int TargetId { get; set; }
    public sbyte TargetOwnerId { get; set; }
    public TargetableArea Area { get; set; } = TargetableArea.AnyAreas;
    public RegionEnum Target { get; set; }
    public sbyte SlotIndex { get; set; }
    public sbyte PushDir { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TargetableArea
{
    Self = 0,
    UnitStack = 1,
    CurrentRegion = 2,
    AnyRegion = 3,
    AnyCommander = 4,
    FriendlyPerimeter = 5,
    EnemyPerimeter = 6,
    Frontline = 7,
    FriendlyRegions = 8,
    EnemyRegions = 9,
    FriendlyCommander = 10,
    EnemyCommander = 11,
    BattleField = 12,
    BattleFieldNC = 13,
    FriendlyHand = 14,
    EnemyHand = 15,
    FriendlyDiscard = 16,
    EnemyDiscard = 17,
    AnyAreas = 18
}

public class GameEventMulliganParams : GameEventParams
{
    public required int[] HandCardIdsToReplace { get; set; }
}

public class GameEventEndTurnParams : GameEventParams
{
    public required int[] HandCardIdsToDiscard { get; set; }
}

public class GameEventMessageParams : GameEventParams
{
    public sbyte MessageId { get; set; }
}

[JsonDerivedType(typeof(InitialSwapEventResult), "InitialSwapEventResult")]
[JsonDerivedType(typeof(DiscardEventResult), "DiscardEventResult")]
public class GameEventResult
{
}

public class InitialSwapEventResult : GameEventResult
{
    public required int[] CardIdsRemovedFromHand { get; set; }
    public required int[] DeckReplacementIndices { get; set; }
}

public class DiscardEventResult : GameEventResult
{
    public required int[] CardIdsRemovedFromHand { get; set; }
}