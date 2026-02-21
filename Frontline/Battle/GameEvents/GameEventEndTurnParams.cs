namespace Frontline.Battle.GameEvents;

public class GameEventEndTurnParams : GameEventParams
{
    public int[] HandCardIdsToDiscard;

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
        DiscardEventResult discardEventResult = (DiscardEventResult) eventResult;
        discardEventResult = new DiscardEventResult();
        discardEventResult.CardIdsRemovedFromHand = HandCardIdsToDiscard;

        if (game.EndTurn(playerIndex, true, HandCardIdsToDiscard) == 1)
        {
            ccgEventsLog = game.GameState.GetCCGEventLog();
            return discardEventResult;
        }

        return null;
    }
}