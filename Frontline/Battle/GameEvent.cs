using System.Text.Json.Serialization;

namespace Frontline.Battle;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GameEvent
{
    Deploy,
    Attack,
    Move,
    ActivateTrait,
    DoInitialSwap,
    EndTurn,
    Surrender,
    Message,
    DiscardCard,
    Disembark,
    TriggerEndTurnTraits,
    [JsonStringEnumMemberName("Cheat_GiveCardAndCmdPts")]
    CheatGiveCardAndCmdPts,
    Interactions,
    NumEvents
}