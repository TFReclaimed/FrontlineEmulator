using System.Text.Json.Serialization;

namespace Frontline.Battle.GameEvents;

public class GameEventRegionTarget : GameEventCardParams
{
    [JsonPropertyName("targetID")]
    public int TargetId { get; }

    [JsonPropertyName("targetOwnerID")]
    public sbyte TargetOwnerId { get; }

    public TargetableArea Area { get; } = TargetableArea.AnyAreas;

    public RegionEnum Target { get; } = RegionEnum.NumRegions;

    public sbyte SlotIndex { get; }

    public sbyte PushDir { get; }

    public GameEventRegionTarget()
    {
    }

    public GameEventRegionTarget(GameEvent gameEv, int cardId, sbyte player, int targetId, sbyte ownerId,
        TargetableArea targetArea, RegionEnum targetRegion, sbyte targetSlot, sbyte dir)
        : base(gameEv, cardId, player)
    {
        TargetId = targetId;
        TargetOwnerId = ownerId;
        Area = targetArea;
        Target = targetRegion;
        SlotIndex = targetSlot;
        PushDir = dir;
        CcgEventsLog = null;
    }

    public override GameEventResult ReplayEvent(CcgGame game)
    {
        bool flag = true;
        if (GameEvent == GameEvent.Deploy)
        {
            if (game.Deploy(PlayerIndex, ActingCardId, TargetOwnerId, TargetId, Area, Target, SlotIndex, PushDir,
                    true) != 1)
            {
                flag = false;
            }
        }
        else if (GameEvent == GameEvent.Attack)
        {
            if (game.Attack(PlayerIndex, ActingCardId, TargetOwnerId, TargetId, true) != 1)
            {
                flag = false;
            }
        }
        else if (GameEvent == GameEvent.Move)
        {
            if (game.Move(PlayerIndex, ActingCardId, Target, SlotIndex, PushDir, true) != 1)
            {
                flag = false;
            }
        }
        else if (GameEvent == GameEvent.ActivateTrait)
        {
            if (game.ActivateTrait(PlayerIndex, ActingCardId, TargetOwnerId, TargetId, Area, Target, true) != 1)
            {
                flag = false;
            }
        }

        CcgEventsLog = game.GameState.GetCCGEventLog();
        return (!flag) ? null : GameEventResult.OK_RESULT;
    }
}