namespace Frontline.Battle.GameEvents;

public class GameEventCardParams : GameEventParams
{
    public int ActingCardId { get; }

    public GameEventCardParams()
    {
    }

    public GameEventCardParams(GameEvent gameEv, int cardId, sbyte player)
        : base(gameEv, player)
    {
        ActingCardId = cardId;
    }

    public override GameEventResult ReplayEvent(CcgGame game)
    {
        if (GameEvent == GameEvent.Disembark)
        {
            if (game.Disembark(PlayerIndex, ActingCardId, true) != 1)
            {
                return null;
            }
        }

        CcgEventsLog = game.GameState.GetCCGEventLog();
        return GameEventResult.OK_RESULT;
    }
}