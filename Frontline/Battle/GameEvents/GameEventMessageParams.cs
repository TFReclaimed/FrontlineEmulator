namespace Frontline.Battle.GameEvents;

public class GameEventMessageParams : GameEventParams
{
    public sbyte messageId;

    public GameEventMessageParams()
    {
    }

    public GameEventMessageParams(GameEvent gameEv, sbyte player, sbyte id)
        : base(gameEv, player)
    {
        messageId = id;
    }

    public override GameEventResult ReplayEvent(CcgGame game)
    {
        if (game.SendMessage(playerIndex, messageId, true) != 1)
        {
            return null;
        }

        return GameEventResult.OK_RESULT;
    }
}