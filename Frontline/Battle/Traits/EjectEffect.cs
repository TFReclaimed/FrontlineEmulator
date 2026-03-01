using Frontline.Battle.CcgEvents;
using Frontline.Game.Card;

namespace Frontline.Battle.Traits;

public class EjectEffect : BaseTraitEffect
{
    public override void CardDied(Card deadCard, Card source, ActiveTrait active)
    {
        var list = GameState.FindCardStack(deadCard);
        if (list.Count <= 0 || list[0].PrimaryCard == null || !list[0].PrimaryCard.HasPilot() ||
            (DurationData.Charges > 0 && active.DurationData.Charges == 0))
        {
            return;
        }

        var cardStack = list[0];
        var unitCard = (UnitCard) cardStack.PrimaryCard;
        var embarkedPilot = unitCard.EmbarkedPilot;
        if ((unitCard.EqualsTo(active.GetTraitTarget()) || embarkedPilot.EqualsTo(active.GetTraitTarget())) &&
            unitCard.GetTemplate().Type == CardType.Titan && embarkedPilot.GetTemplate().Type == CardType.Pilot)
        {
            var logData = new TraitInfoCcgEvent(CcgEventType.TraitEvent, TraitParentId, EffectTraitId,
                active.GetTraitTarget().InstanceId, active.GetTraitTarget().ActiveData.Owner,
                active.GetTraitSource().InstanceId, active.GetTraitSource().ActiveData.Owner, 17);
            GameState.AddCcgEventLog(logData);
            GameState.Disembark(unitCard.ActiveData.Owner, unitCard.InstanceId, true, this);
            if (active.HasCharges())
            {
                active.ExpendCharge();
            }
        }
    }
}