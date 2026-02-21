using System.Text.Json.Serialization;
using Frontline.Battle.CcgEvents;

namespace Frontline.Battle.GameEvents;

[JsonDerivedType(typeof(GameEventCardParams), "GameEventCardParams")]
[JsonDerivedType(typeof(GameEventCheat_GiveCardAndCmdPts), "GameEventCheat_GiveCardAndCmdPts")]
[JsonDerivedType(typeof(GameEventDiscardParams), "GameEventDiscardParams")]
[JsonDerivedType(typeof(GameEventEndTurnParams), "GameEventEndTurnParams")]
[JsonDerivedType(typeof(GameEventInteractionsParams), "GameEventInteractionsParams")]
[JsonDerivedType(typeof(GameEventMessageParams), "GameEventMessageParams")]
[JsonDerivedType(typeof(GameEventMulliganParams), "GameEventMulliganParams")]
[JsonDerivedType(typeof(GameEventRegionTarget), "GameEventRegionTarget")]
public class GameEventParams
{
    public sbyte PlayerIndex { get; } = -1;

    public GameEvent GameEvent { get; set; } = GameEvent.NumEvents;

    public GameEventResult EventResult { get; }

    public List<CCGEventData> CcgEventsLog { get; set; }

    public GameEventParams()
    {
    }

    public GameEventParams(GameEvent gameEv, sbyte player)
    {
        PlayerIndex = player;
        GameEvent = gameEv;
    }

    public virtual GameEventResult ReplayEvent(CcgGame game)
    {
        if (GameEvent == GameEvent.Surrender)
        {
            if (game.Surrender(PlayerIndex, true) != 1)
            {
                return null;
            }
        }
        else if (GameEvent == GameEvent.TriggerEndTurnTraits && game.TriggerEndTurnTraits(PlayerIndex, true) != 1)
        {
            return null;
        }

        return GameEventResult.OK_RESULT;
    }
}