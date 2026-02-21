namespace Frontline.Battle.GameEvents;

public class GameEventDiscardParams : GameEventParams
{
    public int[] HandCardIdsToDiscard { get; set; }

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
        DiscardEventResult discardEventResult = (DiscardEventResult) EventResult;
        discardEventResult = new DiscardEventResult();
        discardEventResult.CardIdsRemovedFromHand = HandCardIdsToDiscard;

        if (game.DoCardDiscard(PlayerIndex, HandCardIdsToDiscard, true) == 1)
        {
            return discardEventResult;
        }

        return null;
    }
}