namespace Frontline.Battle.Traits;

public class CommandModEffect : BaseTraitEffect
{
    public sbyte points;

    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        sbyte b = points;
        if (active.DataValue > 0)
        {
            b = (sbyte) active.DataValue;
        }

        GameState.Players[card.ActiveData.Owner].Resources.AddCommandPoints(b, GameState.GetGameTemplate());
    }

    public override void OnNewTurnEvent(Card owner, sbyte playerIndex)
    {
        if (!owner.IsCardTraitsDetered() && DurationData.Type == TraitDurationType.Permanent &&
            owner.ActiveData.Owner == playerIndex)
        {
            RegionEnum region = RegionEnum.NumRegions;
            CardStack target = GameState.FindCardStack(owner)[0];
            if (Targets.Area == TargetableArea.CurrentRegion)
            {
                region = GameState.GetTraitActorRegion(playerIndex, owner.InstanceId);
            }

            Activate(owner, target, region);
        }
    }
}