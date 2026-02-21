namespace Frontline.Battle.GameEvents;

public class GameEventMessageParams : GameEventParams
{
    public sbyte MessageId { get; }

    public GameEventMessageParams()
    {
    }

    public GameEventMessageParams(GameEvent gameEv, sbyte player, sbyte id)
        : base(gameEv, player)
    {
        MessageId = id;
    }

    public override GameEventResult ReplayEvent(CcgGame game)
    {
        if (game.SendMessage(PlayerIndex, MessageId, true) != 1)
        {
            return null;
        }

        return GameEventResult.OK_RESULT;
    }
}