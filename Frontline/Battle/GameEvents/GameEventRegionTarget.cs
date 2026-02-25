using System.Text.Json.Serialization;
using Frontline.Battle.GameEvents.Result;

namespace Frontline.Battle.GameEvents;

public class GameEventRegionTarget : GameEventCardParams
{
    [JsonPropertyName("targetID")]
    public int TargetId { get; set; }

    [JsonPropertyName("targetOwnerID")]
    public sbyte TargetOwnerId { get; set; }

    public TargetableArea Area { get; set; } = TargetableArea.AnyAreas;

    public Region Target { get; set; } = Region.NumRegions;

    public sbyte SlotIndex { get; set; }

    public sbyte PushDir { get; set; }

    public override GameEventResult ReplayEvent(CcgGame game)
    {
        var success = true;
        if (GameEvent == GameEvent.Deploy)
        {
            if (game.Deploy(PlayerIndex, ActingCardId, TargetOwnerId, TargetId, Area, Target, SlotIndex, PushDir,
                    true) != 1)
            {
                success = false;
            }
        }
        else if (GameEvent == GameEvent.Attack)
        {
            if (game.Attack(PlayerIndex, ActingCardId, TargetOwnerId, TargetId, true) != 1)
            {
                success = false;
            }
        }
        else if (GameEvent == GameEvent.Move)
        {
            if (game.Move(PlayerIndex, ActingCardId, Target, SlotIndex, PushDir, true) != 1)
            {
                success = false;
            }
        }
        else if (GameEvent == GameEvent.ActivateTrait)
        {
            if (game.ActivateTrait(PlayerIndex, ActingCardId, TargetOwnerId, TargetId, Area, Target, true) != 1)
            {
                success = false;
            }
        }

        CcgEventsLog = game.GameState.GetCCGEventLog();
        return success ? GameEventResult.OkResult : null;
    }
}