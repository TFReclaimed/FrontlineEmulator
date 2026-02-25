using Frontline.Battle.CcgEvents;
using Frontline.Game.Card;

namespace Frontline.Battle.Traits;

public class EjectEffect : BaseTraitEffect
{
    public override void CardDied(Card deadCard, Card source, ActiveTrait active)
    {
        List<CardStack> list = GameState.FindCardStack(deadCard);
        if (list.Count <= 0 || list[0].PrimaryCard == null || !list[0].PrimaryCard.HasPilot() ||
            (DurationData.Charges > 0 && active.DurationData.Charges == 0))
        {
            return;
        }

        CardStack cardStack = list[0];
        UnitCard unitCard = (UnitCard) cardStack.PrimaryCard;
        UnitCard embarkedPilot = unitCard.EmbarkedPilot;
        if ((unitCard.EqualsTo(active.GetTraitTarget()) || embarkedPilot.EqualsTo(active.GetTraitTarget())) &&
            unitCard.GetTemplate().Type == CardType.Titan && embarkedPilot.GetTemplate().Type == CardType.Pilot)
        {
            TraitInfoCCGEvent logData = new TraitInfoCCGEvent(CcgEventType.TraitEvent, TraitParentId, EffectTraitId,
                active.GetTraitTarget().InstanceId, active.GetTraitTarget().ActiveData.Owner,
                active.GetTraitSource().InstanceId, active.GetTraitSource().ActiveData.Owner, 17);
            GameState.AddCCGEventLog(logData);
            GameState.Disembark(unitCard.ActiveData.Owner, unitCard.InstanceId, true, this);
            if (active.HasCharges())
            {
                active.ExpendCharge(GameState);
            }
        }
    }
}