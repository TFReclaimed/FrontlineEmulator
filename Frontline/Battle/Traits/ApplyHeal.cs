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
        sbyte b = Heal;
        if (active.DataValue > 0)
        {
            b = (sbyte) active.DataValue;
        }

        if (card.GetCurrentHealth(false) > 0)
        {
            b = card.HealDamage(null, b);
        }

        if (b > 0)
        {
            CardTraumaCCGEvent logData = new CardTraumaCCGEvent(CcgEventType.CardHeal, b, source.InstanceId,
                source.ActiveData.Owner, card.InstanceId, card.ActiveData.Owner);
            GameState.AddCCGEventLog(logData);
        }
    }

    public override void OnNewTurnEvent(Card owner, sbyte playerIndex)
    {
        if (!owner.IsCardTraitsDetered() && DurationData.Type == TraitDurationType.Permanent &&
            owner.ActiveData.Owner == playerIndex)
        {
            Region region = Region.NumRegions;
            CardStack target = GameState.FindCardStack(owner)[0];
            if (Targets.Area == TargetableArea.CurrentRegion)
            {
                region = GameState.GetTraitActorRegion(playerIndex, owner.InstanceId);
            }

            Activate(owner, target, region);
        }
    }
}