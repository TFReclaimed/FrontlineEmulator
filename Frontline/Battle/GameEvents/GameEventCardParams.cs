using Frontline.Battle.GameEvents.Result;

namespace Frontline.Battle.GameEvents;

public class GameEventCardParams : GameEventParams
{
    public int ActingCardId { get; set; }

    public override GameEventResult ReplayEvent(CcgGame game)
    {
        if (GameEvent == GameEvent.Disembark)
        {
            if (game.Disembark(PlayerIndex, ActingCardId) != 1)
            {
                return null;
            }
        }

        CcgEventsLog = game.GameState.GetCCGEventLog();
        return GameEventResult.OkResult;
    }
}