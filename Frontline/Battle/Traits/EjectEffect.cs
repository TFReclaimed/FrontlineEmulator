using Frontline.Battle.CcgEvents;
using Frontline.Game.Card;

namespace Frontline.Battle.Traits;

public class EjectEffect : BaseTraitEffect
{
    public override void CardDied(Card deadCard, Card source, ActiveTrait active)
    {
        List<CardStack> list = GameState.FindCardStack(deadCard);
        if (list.Count <= 0 || list[0].primaryCard == null || !list[0].primaryCard.HasPilot() ||
            (durationData.charges > 0 && active.durationData.charges == 0))
        {
            return;
        }

        CardStack cardStack = list[0];
        UnitCard unitCard = (UnitCard) cardStack.primaryCard;
        UnitCard embarkedPilot = unitCard.embarkedPilot;
        if ((unitCard.EqualsTo(active.GetTraitTarget()) || embarkedPilot.EqualsTo(active.GetTraitTarget())) &&
            unitCard.GetTemplate().Type == CardType.Titan && embarkedPilot.GetTemplate().Type == CardType.Pilot)
        {
            TraitInfoCCGEvent logData = new TraitInfoCCGEvent(CCGEventType.TraitEvent, traitParentID, effectTraitID,
                active.GetTraitTarget().instanceId, active.GetTraitTarget().activeData.owner,
                active.GetTraitSource().instanceId, active.GetTraitSource().activeData.owner, 17);
            GameState.AddCCGEventLog(logData);
            GameState.Disembark(unitCard.activeData.owner, unitCard.instanceId, true, this);
            if (active.HasCharges())
            {
                active.ExpendCharge(GameState);
            }
        }
    }
}