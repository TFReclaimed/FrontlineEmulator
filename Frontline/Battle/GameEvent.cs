using System.Text.Json.Serialization;

namespace Frontline.Battle;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GameEvent : byte
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