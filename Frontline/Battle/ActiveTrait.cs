using Frontline.Battle.CcgEvents;
using Frontline.Battle.Traits;
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

    private BaseTraitEffect _trait;

    private Card _traitSource;

    private Card _traitTarget;

    private readonly CcgGameState _gameState;

    public ActiveTrait(CcgGameState gameState, BaseTraitEffect traitInfo, Card targetCard, Card sourceCard,
        TraitDuration duration)
    {
        _gameState = gameState;
        _trait = traitInfo;
        _traitSource = sourceCard;
        _traitTarget = targetCard;
        Source = new ActiveTraitCardInfo
        {
            InstanceId = sourceCard.InstanceId,
            Owner = sourceCard.ActiveData.Owner
        };
        Target = new ActiveTraitCardInfo
        {
            InstanceId = targetCard.InstanceId,
            Owner = targetCard.ActiveData.Owner
        };
        TraitSourceId = _trait.TraitParentId;
        TraitEffectId = _trait.EffectTraitId;
        Detered = false;
        Triggered = false;
        DurationData = new TraitDuration
        {
            Type = duration.Type,
            Duration = duration.Duration,
            Charges = duration.Charges
        };
    }

    public void Init(Card owner)
    {
        var traitEffectsList = RulesetParser.GetTraitEffectsList(TraitSourceId);
        if (traitEffectsList.Count == 0)
        {
            _gameState.Logger.Warning(" INVALID TRAIT! No Trait effects found for trait #" + TraitSourceId);
            return;
        }

        for (var i = 0; i < traitEffectsList.Count; i++)
        {
            if (traitEffectsList[i].EffectTraitId == TraitEffectId)
            {
                Init(traitEffectsList[i], owner);
                break;
            }
        }
    }

    public void Init(BaseTraitEffect newTrait, Card owner)
    {
        _trait = newTrait;

        var owner2 = owner.ActiveData.Owner;
        var instanceId = owner.InstanceId;
        if (Source.InstanceId == instanceId && Source.Owner == owner2)
        {
            _traitSource = owner;
        }
        else
        {
            _traitSource = _gameState.FindTraitActor(Source.Owner, Source.InstanceId);
        }

        if (Target.InstanceId == instanceId && Target.Owner == owner2)
        {
            _traitTarget = owner;
        }
        else
        {
            _traitTarget = _gameState.FindTraitActor(Target.Owner, Target.InstanceId);
        }

        _trait.Init(_traitTarget, _traitSource, this);
    }

    public void Deactivate(bool validCheck)
    {
        _trait.Deactivate(this);
        _traitTarget.ActiveData.ActiveTraits.Remove(this);
        if (validCheck)
        {
            _traitTarget.TestCardDeathState();
        }
    }

    public void NewTurn(Card owner, sbyte playerIndex)
    {
        _trait.NewTurn(this, playerIndex);
        if (_trait.DurationData.Charges > 0 && _trait.DurationData.Type == TraitDurationType.Permanent)
        {
            DurationData.Charges = _trait.DurationData.Charges;
        }

        if (DurationData.Duration <= 0)
        {
            return;
        }

        var owner2 = _traitSource.ActiveData.Owner;
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
        _trait.EndTurn(this, playerIndex);
        if (DurationData.Duration <= 0)
        {
            return;
        }

        var owner2 = _traitSource.ActiveData.Owner;
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

    public void CardMoved(Card card, CardStack target, Region region, Region origin)
    {
        _trait.CardMoved(card, target, region, origin, this);
    }

    public void CardAttacked(Card attacker, Card target)
    {
        _trait.CardAttacked(attacker, target, this);
    }

    public void CardCounterAttacked(Card attacker, Card target)
    {
        _trait.CardCounterAttacked(attacker, target, this);
    }

    public void CardGainedStatus(Card theCard, Card source, ApplyStatusTraitStatusType statusType)
    {
        _trait.CardGainedStatus(theCard, source, statusType, this);
    }

    public void CardDied(Card deadCard, Card source)
    {
        _trait.CardDied(deadCard, source, this);
    }

    public void CardDamaged(Card damagedCard, Card source)
    {
        _trait.CardDamaged(damagedCard, source, this);
    }

    public void CardDrawn(Card drawnCard, bool regularDraw, bool isNewTurn)
    {
        _trait.CardDrawn(drawnCard, regularDraw, isNewTurn, this);
    }

    public void CardDiscardEffect(sbyte playerIndex, int numberOfCards)
    {
        _trait.CardDiscardEffect(playerIndex, numberOfCards, this);
    }

    public void SecretTriggered(Card secret, Card source)
    {
        _trait.SecretTriggered(secret, source, this);
    }

    public void SecretDestroyed(Card secret, Card source)
    {
        _trait.SecretDestroyed(secret, source, this);
    }

    public void TraitEffectActivating(BaseTraitEffect effect, Card source, CardStack target, Region region)
    {
        _trait.TraitEffectActivating(effect, source, target, region, this);
    }

    public void ExpendCharge()
    {
        if (DurationData.Charges > 0)
        {
            DurationData.Charges--;
        }

        var logData = new TraitInfoCcgEvent(CcgEventType.TraitExpendCharge, _trait.TraitParentId,
            _trait.EffectTraitId, _traitTarget.InstanceId, _traitTarget.ActiveData.Owner, _traitSource.InstanceId,
            _traitSource.ActiveData.Owner, DurationData.Charges);
        _gameState.AddCCGEventLog(logData);
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
        if (_traitTarget.GetTemplate().Type == CardType.Pilot)
        {
            var unitCard = (UnitCard) _traitTarget;
            if (unitCard.IsEmbarked())
            {
                return _trait.EmbarkedInherit;
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

        _trait.Embark(this);
    }

    public void Disembark(bool hasDeter)
    {
        if (!hasDeter && Detered)
        {
            Detered = false;
        }

        _trait.Disembark(this);
    }

    public BaseTraitEffect GetTraitInfo()
    {
        return _trait;
    }

    public Card GetTraitSource()
    {
        return _traitSource;
    }

    public Card GetTraitTarget()
    {
        return _traitTarget;
    }
}