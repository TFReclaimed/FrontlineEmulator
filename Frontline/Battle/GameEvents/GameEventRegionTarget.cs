namespace Frontline.Battle.GameEvents;

public class GameEventRegionTarget : GameEventCardParams
{
    public int targetID;

    public sbyte targetOwnerID;

    public TargetableArea area = TargetableArea.AnyAreas;

    public RegionEnum target = RegionEnum.NumRegions;

    public sbyte slotIndex;

    public sbyte pushDir;

    public GameEventRegionTarget()
    {
    }

    public GameEventRegionTarget(GameEvent gameEv, int cardId, sbyte player, int targetId, sbyte ownerId,
        TargetableArea targetArea, RegionEnum targetRegion, sbyte targetSlot, sbyte dir)
        : base(gameEv, cardId, player)
    {
        targetID = targetId;
        targetOwnerID = ownerId;
        area = targetArea;
        target = targetRegion;
        slotIndex = targetSlot;
        pushDir = dir;
        ccgEventsLog = null;
    }

    public override GameEventResult ReplayEvent(CcgGame game)
    {
        bool flag = true;
        if (gameEvent == GameEvent.Deploy)
        {
            if (game.Deploy(playerIndex, actingCardId, targetOwnerID, targetID, area, target, slotIndex, pushDir,
                    true) != 1)
            {
                flag = false;
            }
        }
        else if (gameEvent == GameEvent.Attack)
        {
            if (game.Attack(playerIndex, actingCardId, targetOwnerID, targetID, true) != 1)
            {
                flag = false;
            }
        }
        else if (gameEvent == GameEvent.Move)
        {
            if (game.Move(playerIndex, actingCardId, target, slotIndex, pushDir, true) != 1)
            {
                flag = false;
            }
        }
        else if (gameEvent == GameEvent.ActivateTrait)
        {
            if (game.ActivateTrait(playerIndex, actingCardId, targetOwnerID, targetID, area, target, true) != 1)
            {
                flag = false;
            }
        }

        ccgEventsLog = game.GameState.GetCCGEventLog();
        return (!flag) ? null : GameEventResult.OK_RESULT;
    }
}