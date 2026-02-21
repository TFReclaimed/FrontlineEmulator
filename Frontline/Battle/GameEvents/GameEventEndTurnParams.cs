namespace Frontline.Battle.GameEvents;

public class GameEventEndTurnParams : GameEventParams
{
    public int[] HandCardIdsToDiscard { get; set; }

    public GameEventEndTurnParams()
    {
    }

    public GameEventEndTurnParams(sbyte playerIdx, int[] cardIdsToReplace)
        : base(GameEvent.EndTurn, playerIdx)
    {
        HandCardIdsToDiscard = cardIdsToReplace;
    }

    public override GameEventResult ReplayEvent(CcgGame game)
    {
        DiscardEventResult discardEventResult = (DiscardEventResult) EventResult;
        discardEventResult = new DiscardEventResult();
        discardEventResult.CardIdsRemovedFromHand = HandCardIdsToDiscard;

        if (game.EndTurn(PlayerIndex, true, HandCardIdsToDiscard) == 1)
        {
            CcgEventsLog = game.GameState.GetCCGEventLog();
            return discardEventResult;
        }

        return null;
    }
}