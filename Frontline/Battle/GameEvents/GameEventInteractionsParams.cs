using System.Text.Json.Serialization;

namespace Frontline.Battle.GameEvents;

public class GameEventInteractionsParams : GameEventParams
{
    public const string InteractionTitleMarker = "INTERACTIONEVENT";

    public int Type { get; }

    public TargetableArea Location { get; }

    [JsonPropertyName("instanceID")]
    public int InstanceId { get; }

    public GameEventInteractionsParams()
    {
        GameEvent = GameEvent.Interactions;
    }

    public override GameEventResult ReplayEvent(CcgGame game)
    {
        return GameEventResult.OK_RESULT;
    }
}