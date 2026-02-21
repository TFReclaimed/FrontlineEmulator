namespace Frontline.Battle.Traits;

public class RemoveStatus : BaseTraitEffect
{
    public sbyte statusType;

    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        card.RemoveStatusEffect(statusType);
    }

    public override void OnNewTurnEvent(Card owner, sbyte playerIndex)
    {
        if (!owner.IsCardTraitsDetered() && durationData.type == TraitDurationType.Permanent &&
            owner.activeData.owner == playerIndex)
        {
            RegionEnum region = RegionEnum.NumRegions;
            CardStack target = GameState.FindCardStack(owner)[0];
            if (targets.area == TargetableArea.CurrentRegion)
            {
                region = GameState.GetTraitActorRegion(playerIndex, owner.instanceId);
            }

            Activate(owner, target, region);
        }
    }
}