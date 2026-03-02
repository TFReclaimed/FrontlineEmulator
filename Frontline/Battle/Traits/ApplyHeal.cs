using Frontline.Battle.CcgEvents;

namespace Frontline.Battle.Traits;

public class ApplyHeal : BaseTraitEffect
{
    public sbyte Heal { get; set; }

    public override bool IsDamageHeal(bool damage)
    {
        return !damage;
    }

    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        var delta = Heal;
        if (active.DataValue > 0)
        {
            delta = (sbyte) active.DataValue;
        }

        if (card.GetCurrentHealth(false) > 0)
        {
            delta = card.HealDamage(null, delta);
        }

        if (delta <= 0)
        {
            return;
        }

        var traumaEvent = new CardTraumaCcgEvent(CcgEventType.CardHeal, delta, source.InstanceId,
            source.ActiveData.Owner, card.InstanceId, card.ActiveData.Owner);
        GameState.AddCcgEventLog(traumaEvent);
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