namespace Frontline.Battle.Traits;

public class CommandModEffect : BaseTraitEffect
{
    public sbyte points;

    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        sbyte b = points;
        if (active.dataValue > 0)
        {
            b = (sbyte) active.dataValue;
        }

        GameState.players[card.activeData.owner].resources.AddCommandPoints(b, GameState.GetGameTemplate());
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