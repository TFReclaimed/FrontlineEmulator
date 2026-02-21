namespace Frontline.Battle.GameEvents;

public class GameEventCardParams : GameEventParams
{
    public int actingCardId;

    public GameEventCardParams()
    {
    }

    public GameEventCardParams(GameEvent gameEv, int cardId, sbyte player)
        : base(gameEv, player)
    {
        actingCardId = cardId;
    }

    public override GameEventResult ReplayEvent(CcgGame game)
    {
        if (gameEvent == GameEvent.Disembark)
        {
            if (game.Disembark(playerIndex, actingCardId, true) != 1)
            {
                return null;
            }
        }

        ccgEventsLog = game.GameState.GetCCGEventLog();
        return GameEventResult.OK_RESULT;
    }
}