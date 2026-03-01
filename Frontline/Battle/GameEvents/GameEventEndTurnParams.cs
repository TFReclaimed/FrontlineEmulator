using Frontline.Battle.GameEvents.Result;

namespace Frontline.Battle.GameEvents;

public class GameEventEndTurnParams : GameEventParams
{
    public int[] HandCardIdsToDiscard { get; set; } = [];

    public override GameEventResult? ReplayEvent(CcgGame game)
    {
        var result = new DiscardEventResult
        {
            CardIdsRemovedFromHand = HandCardIdsToDiscard
        };

        if (game.EndTurn(PlayerIndex, HandCardIdsToDiscard) == 1)
        {
            CcgEventsLog = game.GameState.GetCcgEventLog();
            return result;
        }

        return null;
    }
}