using Frontline.Battle.CcgEvents;
using Frontline.Game;
using Frontline.Game.Card;

namespace Frontline.Battle;

public class ActiveTrait
{
    public TraitDuration DurationData { get; set; }

    public int TraitSourceId { get; set; }

    public int TraitEffectId { get; set; }

    public int DataValue { get; set; }

    public ActiveTraitCardInfo Source { get; set; }

    public ActiveTraitCardInfo Target { get; set; }

    public bool Detered { get; set; }

    public bool Triggered { get; set; }

    private BaseTraitEffect trait;

    private Card traitSource;

    private Card traitTarget;

    public void Init(BaseTraitEffect traitInfo, Card targetCard, Card sourceCard, TraitDuration duration)
    {
        trait = traitInfo;
        if (trait == null)
        {
            Console.WriteLine(" INVALID TRAIT! null effect data for trait #" + TraitSourceId);
            trait = new BaseTraitEffect();
        }

        traitSource = sourceCard;
        traitTarget = targetCard;
        Source = new ActiveTraitCardInfo();
        Source.InstanceId = sourceCard.InstanceId;
        Source.Owner = sourceCard.ActiveData.Owner;
        Target = new ActiveTraitCardInfo();
        Target.InstanceId = targetCard.InstanceId;
        Target.Owner = targetCard.ActiveData.Owner;
        TraitSourceId = trait.TraitParentId;
        TraitEffectId = trait.EffectTraitId;
        Detered = false;
        Triggered = false;
        DurationData = null;
        if (duration != null)
        {
            DurationData = new TraitDuration();
            DurationData.Type = duration.Type;
            DurationData.Duration = duration.Duration;
            DurationData.Charges = duration.Charges;
        }
    }

    public void Init(CCG game, Card owner)
    {
        List<BaseTraitEffect> traitEffectsList = RulesetParser.GetTraitEffectsList(TraitSourceId);
        if (traitEffectsList == null)
        {
            Console.WriteLine(" INVALID TRAIT! No Trait effects found for trait #" + TraitSourceId);
            Init(new BaseTraitEffect(), game, owner);
            return;
        }

        for (int i = 0; i < traitEffectsList.Count; i++)
        {
            if (traitEffectsList[i].EffectTraitId == TraitEffectId)
            {
                Init(traitEffectsList[i], game, owner);
                break;
            }
        }
    }

    public void Init(BaseTraitEffect newTrait, CCG game, Card owner)
    {
        trait = newTrait;
        if (trait == null)
        {
            Console.WriteLine(" INVALID TRAIT! null effect data for trait #" + TraitSourceId);
            trait = new BaseTraitEffect();
        }

        sbyte owner2 = owner.ActiveData.Owner;
        int instanceId = owner.InstanceId;
        if (Source.InstanceId == instanceId && Source.Owner == owner2)
        {
            traitSource = owner;
        }
        else
        {
            traitSource = game.FindTraitActor(Source.Owner, Source.InstanceId);
        }

        if (Target.InstanceId == instanceId && Target.Owner == owner2)
        {
            traitTarget = owner;
        }
        else
        {
            traitTarget = game.FindTraitActor(Target.Owner, Target.InstanceId);
        }

        trait.Init(traitTarget, traitSource, this);
    }

    public void Deactivate(bool validCheck)
    {
        trait.Deactivate(this);
        traitTarget.ActiveData.ActiveTraits.Remove(this);
        if (validCheck)
        {
            traitTarget.TestCardDeathState();
        }
    }

    public void NewTurn(Card owner, sbyte playerIndex)
    {
        trait.NewTurn(this, playerIndex);
        if (trait.DurationData.Charges > 0 && trait.DurationData.Type == TraitDurationType.Permanent)
        {
            DurationData.Charges = trait.DurationData.Charges;
        }

        if (DurationData.Duration <= 0)
        {
            return;
        }

        sbyte owner2 = traitSource.ActiveData.Owner;
        if (DurationData.Type == TraitDurationType.StartOfTurn)
        {
            DurationData.Duration--;
            if (DurationData.Duration == 0)
            {
                Deactivate(true);
            }
        }
        else if (DurationData.Type == TraitDurationType.StartOfMyTurn && owner2 == playerIndex)
        {
            DurationData.Duration--;
            if (DurationData.Duration == 0)
            {
                Deactivate(true);
            }
        }
        else if (DurationData.Type == TraitDurationType.StartOfEnemyTurn && owner2 != playerIndex)
        {
            DurationData.Duration--;
            if (DurationData.Duration == 0)
            {
                Deactivate(true);
            }
        }
    }

    public void EndTurn(Card owner, sbyte playerIndex)
    {
        trait.EndTurn(this, playerIndex);
        if (DurationData.Duration <= 0)
        {
            return;
        }

        sbyte owner2 = traitSource.ActiveData.Owner;
        if (DurationData.Type == TraitDurationType.EndOfTurn)
        {
            DurationData.Duration--;
            if (DurationData.Duration == 0)
            {
                Deactivate(true);
            }
        }
        else if (DurationData.Type == TraitDurationType.EndOfMyTurn && owner2 == playerIndex)
        {
            DurationData.Duration--;
            if (DurationData.Duration == 0)
            {
                Deactivate(true);
            }
        }
        else if (DurationData.Type == TraitDurationType.EndOfEnemyTurn && owner2 != playerIndex)
        {
            DurationData.Duration--;
            if (DurationData.Duration == 0)
            {
                Deactivate(true);
            }
        }
    }

    public void CardMoved(Card card, CardStack target, RegionEnum region, RegionEnum origin)
    {
        trait.CardMoved(card, target, region, origin, this);
    }

    public void CardAttacked(Card attacker, Card target)
    {
        trait.CardAttacked(attacker, target, this);
    }

    public void CardCounterAttacked(Card attacker, Card target)
    {
        trait.CardCounterAttacked(attacker, target, this);
    }

    public void CardGainedStatus(Card theCard, Card source, sbyte statusType)
    {
        trait.CardGainedStatus(theCard, source, statusType, this);
    }

    public void CardDied(Card deadCard, Card source)
    {
        trait.CardDied(deadCard, source, this);
    }

    public void CardDamaged(Card damagedCard, Card source)
    {
        trait.CardDamaged(damagedCard, source, this);
    }

    public void CardDrawn(Card drawnCard, bool regularDraw, bool isNewTurn)
    {
        trait.CardDrawn(drawnCard, regularDraw, isNewTurn, this);
    }

    public void CardDiscardEffect(sbyte playerIndex, int numberOfCards)
    {
        trait.CardDiscardEffect(playerIndex, numberOfCards, this);
    }

    public void SecretTriggered(Card secret, Card source)
    {
        trait.SecretTriggered(secret, source, this);
    }

    public void SecretDestroyed(Card secret, Card source)
    {
        trait.SecretDestroyed(secret, source, this);
    }

    public void TraitEffectActivating(BaseTraitEffect effect, Card source, CardStack target, RegionEnum region)
    {
        trait.TraitEffectActivating(effect, source, target, region, this);
    }

    public void ExpendCharge(CCG gameState)
    {
        if (DurationData.Charges > 0)
        {
            DurationData.Charges--;
        }

        TraitInfoCCGEvent logData = new TraitInfoCCGEvent(CCGEventType.TraitExpendCharge, trait.TraitParentId,
            trait.EffectTraitId, traitTarget.InstanceId, traitTarget.ActiveData.Owner, traitSource.InstanceId,
            traitSource.ActiveData.Owner, DurationData.Charges);
        gameState.AddCCGEventLog(logData);
        if (DurationData.Charges == 0 && DurationData.Type != TraitDurationType.Permanent)
        {
            Deactivate(true);
        }
    }

    public bool HasCharges()
    {
        return DurationData.Charges > 0;
    }

    public bool HasDuration()
    {
        if (DurationData.Type == TraitDurationType.Instant)
        {
            return false;
        }

        if (DurationData.Type == TraitDurationType.Permanent)
        {
            return true;
        }

        return DurationData.Duration > 0;
    }

    public bool EmbarkedCheck()
    {
        if (traitTarget.GetTemplate().Type == CardType.Pilot)
        {
            UnitCard unitCard = (UnitCard) traitTarget;
            if (unitCard.IsEmbarked())
            {
                return trait.EmbarkedInherit;
            }
        }

        return true;
    }

    public void Embark()
    {
        if (!Detered)
        {
            Detered = !EmbarkedCheck();
        }

        trait.Embark(this);
    }

    public void Disembark(bool hasDeter)
    {
        if (!hasDeter && Detered)
        {
            Detered = false;
        }

        trait.Disembark(this);
    }

    public BaseTraitEffect GetTraitInfo()
    {
        return trait;
    }

    public Card GetTraitSource()
    {
        return traitSource;
    }

    public Card GetTraitTarget()
    {
        return traitTarget;
    }
}