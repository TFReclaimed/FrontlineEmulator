namespace Frontline.Battle.GameEvents;

public class GameEventDiscardParams : GameEventParams
{
    public int[] HandCardIdsToDiscard;

    public GameEventDiscardParams()
    {
    }

    public GameEventDiscardParams(int[] cardIdsToReplace, sbyte playerIdx)
        : base(GameEvent.DiscardCard, playerIdx)
    {
        HandCardIdsToDiscard = cardIdsToReplace;
    }

    public override GameEventResult ReplayEvent(CcgGame game)
    {
        DiscardEventResult discardEventResult = (DiscardEventResult) eventResult;
        discardEventResult = new DiscardEventResult();
        discardEventResult.CardIdsRemovedFromHand = HandCardIdsToDiscard;

        if (game.DoCardDiscard(playerIndex, HandCardIdsToDiscard, true) == 1)
        {
            return discardEventResult;
        }

        return null;
    }
}