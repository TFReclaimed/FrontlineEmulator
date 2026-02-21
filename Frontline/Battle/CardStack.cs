namespace Frontline.Battle;

public class CardStack
{
    public Card primaryCard;

    private EntityCard ejectedCard;

    public void Create()
    {
        primaryCard = null;
        ejectedCard = null;
    }

    public void Init(CCG game)
    {
        if (primaryCard != null)
        {
            primaryCard = primaryCard.GenerateAndInit(game);
        }
    }

    public void InitActiveData()
    {
        if (primaryCard != null)
        {
            primaryCard.InitActiveData();
        }
    }

    public bool HasCard(Card card)
    {
        if (card == null)
        {
            Console.WriteLine("CARDSTACK ERROR - Trying to Find a NULL card");
            return false;
        }

        return HasCard(card.instanceId, card.activeData.owner);
    }

    public bool HasCard(int cardId, sbyte ownerId)
    {
        return FindTraitActor(cardId, ownerId) != null;
    }

    public Card FindTraitActor(int cardId, sbyte ownerId)
    {
        if (primaryCard == null)
        {
            return null;
        }

        Card card = primaryCard.FindTraitActor(cardId, ownerId);
        if (card != null)
        {
            return card;
        }

        if (ejectedCard != null)
        {
            return ejectedCard.FindTraitActor(cardId, ownerId);
        }

        return null;
    }

    public void FindCards(TraitTargeting info, Card source, List<CardStack> found)
    {
        if (primaryCard != null &&
            ((info.scope != TraitTargetScope.AllFriendlyNotSelf && info.scope != TraitTargetScope.FriendlyUnitNotSelf &&
              info.scope != TraitTargetScope.RandomFriendlyNotSelf) || !source.EqualsTo(primaryCard)))
        {
            if (primaryCard.DoesMatchTargetingInfo(info, source))
            {
                found.Add(this);
            }
            else if (ejectedCard != null && ejectedCard.DoesMatchTargetingInfo(info, source))
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

    public CardStack FindCard(int cardId, sbyte ownerId)
    {
        if (HasCard(cardId, ownerId))
        {
            return this;
        }

        return null;
    }

    public Card RemoveCard(int cardId, sbyte ownerId)
    {
        Card card = FindTraitActor(cardId, ownerId);
        if (card != null)
        {
            RemoveCard(card);
            return card;
        }

        return null;
    }

    public Card RemoveCard(Card card)
    {
        Card card2 = null;
        if (primaryCard != null)
        {
            if (primaryCard.EqualsTo(card))
            {
                card2 = primaryCard;
                primaryCard = null;
                return card2;
            }

            List<Card> secrets = primaryCard.GetSecrets();
            if (secrets != null)
            {
                for (int i = 0; i < secrets.Count; i++)
                {
                    if (secrets[i].EqualsTo(card))
                    {
                        card2 = secrets[i];
                        secrets.RemoveAt(i);
                        return card2;
                    }
                }
            }

            if (primaryCard.HasPilot())
            {
                card2 = primaryCard.GetEmbarkedPilot();
                if (card2.EqualsTo(card))
                {
                    UnitCard unitCard = (UnitCard) primaryCard;
                    unitCard.embarkedPilot = null;
                    return card2;
                }
            }
        }

        if (ejectedCard != null)
        {
            if (ejectedCard.EqualsTo(card))
            {
                card2 = ejectedCard;
                ejectedCard = null;
                return card2;
            }

            List<Card> secrets2 = ejectedCard.GetSecrets();
            if (secrets2 != null)
            {
                for (int j = 0; j < secrets2.Count; j++)
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
        if (primaryCard != null)
        {
            primaryCard.CardDeployed(deployed);
            if (ejectedCard != null)
            {
                ejectedCard.CardDeployed(deployed);
            }
        }
    }

    public void NewTurn(sbyte playerIndex)
    {
        if (primaryCard != null)
        {
            primaryCard.NewTurn(playerIndex);
        }

        if (ejectedCard != null)
        {
            ejectedCard.NewTurn(playerIndex);
        }
    }

    public void EndTurn(sbyte playerIndex)
    {
        if (primaryCard != null)
        {
            primaryCard.EndTurn(playerIndex);
        }

        if (ejectedCard != null)
        {
            ejectedCard.EndTurn(playerIndex);
        }
    }

    public void CardMoved(Card card, CardStack target, RegionEnum region, RegionEnum origin)
    {
        if (primaryCard != null)
        {
            primaryCard.CardMoved(card, target, region, origin);
        }

        if (ejectedCard != null)
        {
            ejectedCard.CardMoved(card, target, region, origin);
        }
    }

    public void CardGainedStatus(Card theCard, Card source, sbyte statusType)
    {
        if (primaryCard != null)
        {
            primaryCard.CardGainedStatus(theCard, source, statusType);
        }

        if (ejectedCard != null)
        {
            ejectedCard.CardGainedStatus(theCard, source, statusType);
        }
    }

    public void CardAttacked(Card attacker, Card target)
    {
        if (primaryCard != null)
        {
            primaryCard.CardAttacked(attacker, target);
        }

        if (ejectedCard != null)
        {
            ejectedCard.CardAttacked(attacker, target);
        }
    }

    public void CardCounterAttacked(Card attacker, Card target)
    {
        if (primaryCard != null)
        {
            primaryCard.CardCounterAttacked(attacker, target);
        }

        if (ejectedCard != null)
        {
            ejectedCard.CardCounterAttacked(attacker, target);
        }
    }

    public void CardDamaged(Card damagedCard, Card source)
    {
        if (primaryCard != null)
        {
            primaryCard.CardDamaged(damagedCard, source);
        }

        if (ejectedCard != null)
        {
            ejectedCard.CardDamaged(damagedCard, source);
        }
    }

    public void CardDied(Card deadCard, Card source)
    {
        if (primaryCard != null)
        {
            primaryCard.CardDied(deadCard, source);
        }

        if (ejectedCard != null)
        {
            ejectedCard.CardDied(deadCard, source);
        }
    }

    public void CardDrawn(Card drawnCard, bool regularDraw, bool isNewTurn)
    {
        if (primaryCard != null)
        {
            primaryCard.CardDrawn(drawnCard, regularDraw, isNewTurn);
        }

        if (ejectedCard != null)
        {
            ejectedCard.CardDrawn(drawnCard, regularDraw, isNewTurn);
        }
    }

    public void CardDiscardEffect(sbyte playerIndex, int numberOfCards)
    {
        if (primaryCard != null)
        {
            primaryCard.CardDiscardEffect(playerIndex, numberOfCards);
        }

        if (ejectedCard != null)
        {
            ejectedCard.CardDiscardEffect(playerIndex, numberOfCards);
        }
    }

    public void SecretTriggered(Card secret, Card source)
    {
        if (primaryCard != null)
        {
            primaryCard.SecretTriggered(secret, source);
        }

        if (ejectedCard != null)
        {
            ejectedCard.SecretTriggered(secret, source);
        }
    }

    public void SecretDestroyed(Card secret, Card source)
    {
        if (primaryCard != null)
        {
            primaryCard.SecretDestroyed(secret, source);
        }

        if (ejectedCard != null)
        {
            ejectedCard.SecretDestroyed(secret, source);
        }
    }

    public void TraitEffectActivating(BaseTraitEffect effect, Card source, CardStack target, RegionEnum region)
    {
        if (primaryCard != null)
        {
            primaryCard.TraitEffectActivating(effect, source, target, region);
        }

        if (ejectedCard != null)
        {
            ejectedCard.TraitEffectActivating(effect, source, target, region);
        }
    }

    public bool CheckDiscard(Player[] players)
    {
        if (primaryCard != null && primaryCard.CanDiscard())
        {
            primaryCard.Discard(players);
            primaryCard = null;
            if (ejectedCard != null)
            {
                primaryCard = ejectedCard;
                ejectedCard = null;
            }

            return true;
        }

        return false;
    }

    public void SetEjectedCard(Card card)
    {
        if (primaryCard != null && primaryCard.HasPilot())
        {
            UnitCard unitCard = (UnitCard) primaryCard;
            if (unitCard.embarkedPilot.EqualsTo(card))
            {
                unitCard.embarkedPilot.pilotEmbarked = false;
                ejectedCard = unitCard.embarkedPilot;
                unitCard.embarkedPilot = null;
            }
        }
    }

    public EntityCard GetEjectedCard()
    {
        return ejectedCard;
    }
}