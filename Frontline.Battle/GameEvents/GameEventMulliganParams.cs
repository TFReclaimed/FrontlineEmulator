using Frontline.Battle.GameEvents.Result;

namespace Frontline.Battle.GameEvents;

public class GameEventMulliganParams : GameEventParams
{
    public int[] HandCardIdsToReplace { get; set; } = [];

    public override GameEventResult? ReplayEvent(CcgGame game)
    {
        var result = new InitialSwapEventResult
        {
            CardIdsRemovedFromHand = HandCardIdsToReplace,
            DeckReplacementIndices = new int[HandCardIdsToReplace.Length]
        };

        if (game.DoInitialSwap(PlayerIndex, result.CardIdsRemovedFromHand, result.DeckReplacementIndices))
        {
            CcgEventsLog = game.GameState.GetCcgEventLog();
            return result;
        }

        return null;
    }
}