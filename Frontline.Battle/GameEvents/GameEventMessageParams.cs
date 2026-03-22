using Frontline.Battle.GameEvents.Result;

namespace Frontline.Battle.GameEvents;

public class GameEventMessageParams : GameEventParams
{
    public sbyte MessageId { get; set; }

    public override GameEventResult? ReplayEvent(CcgGame game)
    {
        if (!game.SendMessage(PlayerIndex))
        {
            return null;
        }

        return GameEventResult.OkResult;
    }
}