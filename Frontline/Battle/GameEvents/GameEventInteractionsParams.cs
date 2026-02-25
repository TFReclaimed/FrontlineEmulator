using System.Text.Json.Serialization;
using Frontline.Battle.GameEvents.Result;

namespace Frontline.Battle.GameEvents;

public class GameEventInteractionsParams : GameEventParams
{
    public int Type { get; set; }

    public TargetableArea Location { get; set; }

    [JsonPropertyName("instanceID")]
    public int InstanceId { get; set; }

    public GameEventInteractionsParams()
    {
        GameEvent = GameEvent.Interactions;
    }

    public override GameEventResult ReplayEvent(CcgGame game)
    {
        return GameEventResult.OkResult;
    }
}