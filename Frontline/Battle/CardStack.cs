namespace Frontline.Battle;

public class CardStack
{
    public Card? PrimaryCard { get; set; }

    private EntityCard? _ejectedCard;

    public CardStack()
    {
        PrimaryCard = null;
        _ejectedCard = null;
    }

    public void Init(CCG game)
    {
        PrimaryCard = PrimaryCard?.GenerateAndInit(game);
    }

    public void InitActiveData()
    {
        PrimaryCard?.InitActiveData();
    }

    public bool HasCard(Card? card)
    {
        if (card == null)
        {
            Console.WriteLine("CARDSTACK ERROR - Trying to Find a NULL card");
            return false;
        }

        return HasCard(card.InstanceId, card.ActiveData.Owner);
    }

    public bool HasCard(int cardId, sbyte ownerId)
    {
        return FindTraitActor(cardId, ownerId) != null;
    }

    public Card? FindTraitActor(int cardId, sbyte ownerId)
    {
        if (PrimaryCard == null)
        {
            return null;
        }

        var card = PrimaryCard.FindTraitActor(cardId, ownerId);
        if (card != null)
        {
            return card;
        }

        return _ejectedCard?.FindTraitActor(cardId, ownerId);
    }

    public void FindCards(TraitTargeting info, Card source, List<CardStack> found)
    {
        if (PrimaryCard != null &&
            ((info.Scope != TraitTargetScope.AllFriendlyNotSelf && info.Scope != TraitTargetScope.FriendlyUnitNotSelf &&
              info.Scope != TraitTargetScope.RandomFriendlyNotSelf) || !source.EqualsTo(PrimaryCard)))
        {
            if (PrimaryCard.DoesMatchTargetingInfo(info, source) ||
                _ejectedCard != null && _ejectedCard.DoesMatchTargetingInfo(info, source))
            {
                found.Add(this);
            }
        }
    }

    public bool FindCardStack(Card card, List<CardStack> found)
    {
        if (HasCard(card))
        {
            found.Add(this);
            return true;
        }

        return false;
    }

    public CardStack? FindCard(int cardId, sbyte ownerId)
    {
        if (HasCard(cardId, ownerId))
        {
            return this;
        }

        return null;
    }

    public Card? RemoveCard(int cardId, sbyte ownerId)
    {
        var card = FindTraitActor(cardId, ownerId);
        if (card != null)
        {
            RemoveCard(card);
            return card;
        }

        return null;
    }

    public Card? RemoveCard(Card card)
    {
        Card? card2;
        if (PrimaryCard != null)
        {
            if (PrimaryCard.EqualsTo(card))
            {
                card2 = PrimaryCard;
                PrimaryCard = null;
                return card2;
            }

            var secrets = PrimaryCard.GetSecrets();
            if (secrets != null)
            {
                for (var i = 0; i < secrets.Count; i++)
                {
                    if (secrets[i].EqualsTo(card))
                    {
                        card2 = secrets[i];
                        secrets.RemoveAt(i);
                        return card2;
                    }
                }
            }

            if (PrimaryCard.HasPilot())
            {
                card2 = PrimaryCard.GetEmbarkedPilot();
                if (card2.EqualsTo(card))
                {
                    var unitCard = (UnitCard) PrimaryCard;
                    unitCard.EmbarkedPilot = null;
                    return card2;
                }
            }
        }

        if (_ejectedCard != null)
        {
            if (_ejectedCard.EqualsTo(card))
            {
                card2 = _ejectedCard;
                _ejectedCard = null;
                return card2;
            }

            var secrets2 = _ejectedCard.GetSecrets();
            if (secrets2 != null)
            {
                for (var j = 0; j < secrets2.Count; j++)
                {
                    if (secrets2[j].EqualsTo(card))
                    {
                        card2 = secrets2[j];
                        secrets2.RemoveAt(j);
                        return card2;
                    }
                }
            }
        }

        return null;
    }

    public void CardDeployed(Card deployed)
    {
        if (PrimaryCard != null)
        {
            PrimaryCard.CardDeployed(deployed);
            _ejectedCard?.CardDeployed(deployed);
        }
    }

    public void NewTurn(sbyte playerIndex)
    {
        PrimaryCard?.NewTurn(playerIndex);
        _ejectedCard?.NewTurn(playerIndex);
    }

    public void EndTurn(sbyte playerIndex)
    {
        PrimaryCard?.EndTurn(playerIndex);
        _ejectedCard?.EndTurn(playerIndex);
    }

    public void CardMoved(Card card, CardStack target, Region region, Region origin)
    {
        PrimaryCard?.CardMoved(card, target, region, origin);
        _ejectedCard?.CardMoved(card, target, region, origin);
    }

    public void CardGainedStatus(Card theCard, Card source, sbyte statusType)
    {
        PrimaryCard?.CardGainedStatus(theCard, source, statusType);
        _ejectedCard?.CardGainedStatus(theCard, source, statusType);
    }

    public void CardAttacked(Card attacker, Card target)
    {
        PrimaryCard?.CardAttacked(attacker, target);
        _ejectedCard?.CardAttacked(attacker, target);
    }

    public void CardCounterAttacked(Card attacker, Card target)
    {
        PrimaryCard?.CardCounterAttacked(attacker, target);
        _ejectedCard?.CardCounterAttacked(attacker, target);
    }

    public void CardDamaged(Card damagedCard, Card source)
    {
        PrimaryCard?.CardDamaged(damagedCard, source);
        _ejectedCard?.CardDamaged(damagedCard, source);
    }

    public void CardDied(Card deadCard, Card source)
    {
        PrimaryCard?.CardDied(deadCard, source);
        _ejectedCard?.CardDied(deadCard, source);
    }

    public void CardDrawn(Card drawnCard, bool regularDraw, bool isNewTurn)
    {
        PrimaryCard?.CardDrawn(drawnCard, regularDraw, isNewTurn);
        _ejectedCard?.CardDrawn(drawnCard, regularDraw, isNewTurn);
    }

    public void CardDiscardEffect(sbyte playerIndex, int numberOfCards)
    {
        PrimaryCard?.CardDiscardEffect(playerIndex, numberOfCards);
        _ejectedCard?.CardDiscardEffect(playerIndex, numberOfCards);
    }

    public void SecretTriggered(Card secret, Card source)
    {
        PrimaryCard?.SecretTriggered(secret, source);
        _ejectedCard?.SecretTriggered(secret, source);
    }

    public void SecretDestroyed(Card secret, Card source)
    {
        PrimaryCard?.SecretDestroyed(secret, source);
        _ejectedCard?.SecretDestroyed(secret, source);
    }

    public void TraitEffectActivating(BaseTraitEffect effect, Card source, CardStack target, Region region)
    {
        PrimaryCard?.TraitEffectActivating(effect, source, target, region);
        _ejectedCard?.TraitEffectActivating(effect, source, target, region);
    }

    public bool CheckDiscard(Player[] players)
    {
        if (PrimaryCard != null && PrimaryCard.CanDiscard())
        {
            PrimaryCard.Discard(players);
            PrimaryCard = null;
            if (_ejectedCard != null)
            {
                PrimaryCard = _ejectedCard;
                _ejectedCard = null;
            }

            return true;
        }

        return false;
    }

    public void SetEjectedCard(Card card)
    {
        if (PrimaryCard != null && PrimaryCard.HasPilot())
        {
            var unitCard = (UnitCard) PrimaryCard;
            if (unitCard.EmbarkedPilot.EqualsTo(card))
            {
                unitCard.EmbarkedPilot.PilotEmbarked = false;
                _ejectedCard = unitCard.EmbarkedPilot;
                unitCard.EmbarkedPilot = null;
            }
        }
    }

    public EntityCard? GetEjectedCard()
    {
        return _ejectedCard;
    }
}