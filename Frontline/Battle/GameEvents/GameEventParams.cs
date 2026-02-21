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
    public sbyte playerIndex = -1;

    public GameEvent gameEvent = GameEvent.NumEvents;

    public GameEventResult eventResult;

    public List<CCGEventData> ccgEventsLog;

    public GameEventParams()
    {
    }

    public GameEventParams(GameEvent gameEv, sbyte player)
    {
        playerIndex = player;
        gameEvent = gameEv;
    }

    public virtual GameEventResult ReplayEvent(CcgGame game)
    {
        if (gameEvent == GameEvent.Surrender)
        {
            if (game.Surrender(playerIndex, true) != 1)
            {
                return null;
            }
        }
        else if (gameEvent == GameEvent.TriggerEndTurnTraits && game.TriggerEndTurnTraits(playerIndex, true) != 1)
        {
            return null;
        }

        return GameEventResult.OK_RESULT;
    }
}