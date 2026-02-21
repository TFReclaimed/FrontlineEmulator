using Frontline.Battle.CcgEvents;
using Frontline.Game;
using Frontline.Game.Card;

namespace Frontline.Battle;

public class ActiveTrait
{
    public TraitDuration durationData;

    public int traitSourceId;

    public int traitEffectId;

    public int dataValue;

    public ActiveTraitCardInfo source;

    public ActiveTraitCardInfo target;

    public bool detered;

    public bool triggered;

    private BaseTraitEffect trait;

    private Card traitSource;

    private Card traitTarget;

    public void Init(BaseTraitEffect traitInfo, Card targetCard, Card sourceCard, TraitDuration duration)
    {
        trait = traitInfo;
        if (trait == null)
        {
            Console.WriteLine(" INVALID TRAIT! null effect data for trait #" + traitSourceId);
            trait = new BaseTraitEffect();
        }

        traitSource = sourceCard;
        traitTarget = targetCard;
        source = new ActiveTraitCardInfo();
        source.instanceId = sourceCard.instanceId;
        source.owner = sourceCard.activeData.owner;
        target = new ActiveTraitCardInfo();
        target.instanceId = targetCard.instanceId;
        target.owner = targetCard.activeData.owner;
        traitSourceId = trait.traitParentID;
        traitEffectId = trait.effectTraitID;
        detered = false;
        triggered = false;
        durationData = null;
        if (duration != null)
        {
            durationData = new TraitDuration();
            durationData.type = duration.type;
            durationData.duration = duration.duration;
            durationData.charges = duration.charges;
        }
    }

    public void Init(CCG game, Card owner)
    {
        List<BaseTraitEffect> traitEffectsList = RulesetParser.GetTraitEffectsList(traitSourceId);
        if (traitEffectsList == null)
        {
            Console.WriteLine(" INVALID TRAIT! No Trait effects found for trait #" + traitSourceId);
            Init(new BaseTraitEffect(), game, owner);
            return;
        }

        for (int i = 0; i < traitEffectsList.Count; i++)
        {
            if (traitEffectsList[i].effectTraitID == traitEffectId)
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
            Console.WriteLine(" INVALID TRAIT! null effect data for trait #" + traitSourceId);
            trait = new BaseTraitEffect();
        }

        sbyte owner2 = owner.activeData.owner;
        int instanceId = owner.instanceId;
        if (source.instanceId == instanceId && source.owner == owner2)
        {
            traitSource = owner;
        }
        else
        {
            traitSource = game.FindTraitActor(source.owner, source.instanceId);
        }

        if (target.instanceId == instanceId && target.owner == owner2)
        {
            traitTarget = owner;
        }
        else
        {
            traitTarget = game.FindTraitActor(target.owner, target.instanceId);
        }

        trait.Init(traitTarget, traitSource, this);
    }

    public void Deactivate(bool validCheck)
    {
        trait.Deactivate(this);
        traitTarget.activeData.activeTraits.Remove(this);
        if (validCheck)
        {
            traitTarget.TestCardDeathState();
        }
    }

    public void NewTurn(Card owner, sbyte playerIndex)
    {
        trait.NewTurn(this, playerIndex);
        if (trait.durationData.charges > 0 && trait.durationData.type == TraitDurationType.Permanent)
        {
            durationData.charges = trait.durationData.charges;
        }

        if (durationData.duration <= 0)
        {
            return;
        }

        sbyte owner2 = traitSource.activeData.owner;
        if (durationData.type == TraitDurationType.StartOfTurn)
        {
            durationData.duration--;
            if (durationData.duration == 0)
            {
                Deactivate(true);
            }
        }
        else if (durationData.type == TraitDurationType.StartOfMyTurn && owner2 == playerIndex)
        {
            durationData.duration--;
            if (durationData.duration == 0)
            {
                Deactivate(true);
            }
        }
        else if (durationData.type == TraitDurationType.StartOfEnemyTurn && owner2 != playerIndex)
        {
            durationData.duration--;
            if (durationData.duration == 0)
            {
                Deactivate(true);
            }
        }
    }

    public void EndTurn(Card owner, sbyte playerIndex)
    {
        trait.EndTurn(this, playerIndex);
        if (durationData.duration <= 0)
        {
            return;
        }

        sbyte owner2 = traitSource.activeData.owner;
        if (durationData.type == TraitDurationType.EndOfTurn)
        {
            durationData.duration--;
            if (durationData.duration == 0)
            {
                Deactivate(true);
            }
        }
        else if (durationData.type == TraitDurationType.EndOfMyTurn && owner2 == playerIndex)
        {
            durationData.duration--;
            if (durationData.duration == 0)
            {
                Deactivate(true);
            }
        }
        else if (durationData.type == TraitDurationType.EndOfEnemyTurn && owner2 != playerIndex)
        {
            durationData.duration--;
            if (durationData.duration == 0)
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
        if (durationData.charges > 0)
        {
            durationData.charges--;
        }

        TraitInfoCCGEvent logData = new TraitInfoCCGEvent(CCGEventType.TraitExpendCharge, trait.traitParentID,
            trait.effectTraitID, traitTarget.instanceId, traitTarget.activeData.owner, traitSource.instanceId,
            traitSource.activeData.owner, durationData.charges);
        gameState.AddCCGEventLog(logData);
        if (durationData.charges == 0 && durationData.type != TraitDurationType.Permanent)
        {
            Deactivate(true);
        }
    }

    public bool HasCharges()
    {
        return durationData.charges > 0;
    }

    public bool HasDuration()
    {
        if (durationData.type == TraitDurationType.Instant)
        {
            return false;
        }

        if (durationData.type == TraitDurationType.Permanent)
        {
            return true;
        }

        return durationData.duration > 0;
    }

    public bool EmbarkedCheck()
    {
        if (traitTarget.GetTemplate().Type == CardType.Pilot)
        {
            UnitCard unitCard = (UnitCard) traitTarget;
            if (unitCard.IsEmbarked())
            {
                return trait.embarkedInherit;
            }
        }

        return true;
    }

    public void Embark()
    {
        if (!detered)
        {
            detered = !EmbarkedCheck();
        }

        trait.Embark(this);
    }

    public void Disembark(bool hasDeter)
    {
        if (!hasDeter && detered)
        {
            detered = false;
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