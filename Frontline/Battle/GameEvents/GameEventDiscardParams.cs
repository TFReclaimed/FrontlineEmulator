using Frontline.Battle.GameEvents.Result;

namespace Frontline.Battle.GameEvents;

public class GameEventDiscardParams : GameEventParams
{
    public int[] HandCardIdsToDiscard { get; set; }

    public override GameEventResult ReplayEvent(CcgGame game)
    {
        var result = new DiscardEventResult
        {
            CardIdsRemovedFromHand = HandCardIdsToDiscard
        };

        if (game.DoCardDiscard(PlayerIndex, HandCardIdsToDiscard) == 1)
        {
            return result;
        }

        return null;
    }
}