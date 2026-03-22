using System.Text.Json.Serialization;
using Frontline.Battle.CcgEvents;
using Frontline.Battle.GameEvents.Result;

namespace Frontline.Battle.GameEvents;

[JsonDerivedType(typeof(GameEventParams), "GameEventParams")]
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
    public sbyte PlayerIndex { get; set; } = -1;

    public GameEvent GameEvent { get; set; } = GameEvent.NumEvents;

    public GameEventResult? EventResult { get; set; }

    public List<CcgEventData>? CcgEventsLog { get; set; }

    public virtual GameEventResult? ReplayEvent(CcgGame game)
    {
        if (GameEvent == GameEvent.Surrender)
        {
            if (!game.Surrender(PlayerIndex))
            {
                return null;
            }
        }
        else if (GameEvent == GameEvent.TriggerEndTurnTraits && !game.TriggerEndTurnTraits(PlayerIndex))
        {
            return null;
        }

        return GameEventResult.OkResult;
    }
}