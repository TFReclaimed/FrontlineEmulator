namespace Frontline.Battle.Traits;

public class RemoveStatus : BaseTraitEffect
{
    public ApplyStatusTraitStatusType StatusType { get; set; }

    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        card.RemoveStatusEffect(StatusType);
    }

    public override void OnNewTurnEvent(Card owner, sbyte playerIndex)
    {
        if (owner.IsCardTraitsDetered() || DurationData.Type != TraitDurationType.Permanent ||
            owner.ActiveData.Owner != playerIndex)
        {
            return;
        }

        var region = Region.NumRegions;
        var target = GameState.FindCardStack(owner)[0];
        if (Targets.Area == TargetableArea.CurrentRegion)
        {
            region = GameState.GetTraitActorRegion(playerIndex, owner.InstanceId);
        }

        Activate(owner, target, region);
    }
}