namespace Frontline.Battle.GameEvents;

public class GameEventMulliganParams : GameEventParams
{
    public int[] HandCardIdsToReplace { get; set; }

    public GameEventMulliganParams()
    {
    }

    public GameEventMulliganParams(int[] cardIdsToReplace, sbyte playerIdx)
        : base(GameEvent.DoInitialSwap, playerIdx)
    {
        HandCardIdsToReplace = cardIdsToReplace;
    }

    public override GameEventResult ReplayEvent(CcgGame game)
    {
        InitialSwapEventResult initialSwapEventResult = (InitialSwapEventResult) EventResult;
        initialSwapEventResult = new InitialSwapEventResult();
        initialSwapEventResult.CardIdsRemovedFromHand = HandCardIdsToReplace;
        initialSwapEventResult.DeckReplacementIndices = new int[HandCardIdsToReplace.Length];

        if (game.DoInitialSwap(PlayerIndex, initialSwapEventResult.CardIdsRemovedFromHand,
                initialSwapEventResult.DeckReplacementIndices, true) == 1)
        {
            CcgEventsLog = game.GameState.GetCCGEventLog();
            return initialSwapEventResult;
        }

        return null;
    }
}