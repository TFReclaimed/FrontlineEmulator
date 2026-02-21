namespace Frontline.Battle.GameEvents;

public class GameEventInteractionsParams : GameEventParams
{
    public const string InteractionTitleMarker = "INTERACTIONEVENT";

    public int type;

    public TargetableArea location;

    public int instanceID;

    public GameEventInteractionsParams()
    {
        gameEvent = GameEvent.Interactions;
    }

    public override GameEventResult ReplayEvent(CcgGame game)
    {
        return GameEventResult.OK_RESULT;
    }
}