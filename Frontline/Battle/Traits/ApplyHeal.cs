using Frontline.Battle.CcgEvents;

namespace Frontline.Battle.Traits;

public class ApplyHeal : BaseTraitEffect
{
    public sbyte heal;

    public override bool IsDamageHeal(bool damage)
    {
        return !damage;
    }

    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        sbyte b = heal;
        if (active.dataValue > 0)
        {
            b = (sbyte) active.dataValue;
        }

        if (card.GetCurrentHealth(false) > 0)
        {
            b = card.HealDamage(null, b);
        }

        if (b > 0)
        {
            CardTraumaCCGEvent logData = new CardTraumaCCGEvent(CCGEventType.CardHeal, b, source.instanceId,
                source.activeData.owner, card.instanceId, card.activeData.owner);
            GameState.AddCCGEventLog(logData);
        }
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