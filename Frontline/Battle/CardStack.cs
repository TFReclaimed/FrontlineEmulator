namespace Frontline.Battle;

public class CardStack
{
    public Card PrimaryCard { get; set; }

    private EntityCard ejectedCard;

    public void Create()
    {
        PrimaryCard = null;
        ejectedCard = null;
    }

    public void Init(CCG game)
    {
        if (PrimaryCard != null)
        {
            PrimaryCard = PrimaryCard.GenerateAndInit(game);
        }
    }

    public void InitActiveData()
    {
        if (PrimaryCard != null)
        {
            PrimaryCard.InitActiveData();
        }
    }

    public bool HasCard(Card card)
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

    public Card FindTraitActor(int cardId, sbyte ownerId)
    {
        if (PrimaryCard == null)
        {
            return null;
        }

        Card card = PrimaryCard.FindTraitActor(cardId, ownerId);
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
        if (PrimaryCard != null &&
            ((info.Scope != TraitTargetScope.AllFriendlyNotSelf && info.Scope != TraitTargetScope.FriendlyUnitNotSelf &&
              info.Scope != TraitTargetScope.RandomFriendlyNotSelf) || !source.EqualsTo(PrimaryCard)))
        {
            if (PrimaryCard.DoesMatchTargetingInfo(info, source))
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
        if (PrimaryCard != null)
        {
            if (PrimaryCard.EqualsTo(card))
            {
                card2 = PrimaryCard;
                PrimaryCard = null;
                return card2;
            }

            List<Card> secrets = PrimaryCard.GetSecrets();
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

            if (PrimaryCard.HasPilot())
            {
                card2 = PrimaryCard.GetEmbarkedPilot();
                if (card2.EqualsTo(card))
                {
                    UnitCard unitCard = (UnitCard) PrimaryCard;
                    unitCard.EmbarkedPilot = null;
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
        if (PrimaryCard != null)
        {
            PrimaryCard.CardDeployed(deployed);
            if (ejectedCard != null)
            {
                ejectedCard.CardDeployed(deployed);
            }
        }
    }

    public void NewTurn(sbyte playerIndex)
    {
        if (PrimaryCard != null)
        {
            PrimaryCard.NewTurn(playerIndex);
        }

        if (ejectedCard != null)
        {
            ejectedCard.NewTurn(playerIndex);
        }
    }

    public void EndTurn(sbyte playerIndex)
    {
        if (PrimaryCard != null)
        {
            PrimaryCard.EndTurn(playerIndex);
        }

        if (ejectedCard != null)
        {
            ejectedCard.EndTurn(playerIndex);
        }
    }

    public void CardMoved(Card card, CardStack target, RegionEnum region, RegionEnum origin)
    {
        if (PrimaryCard != null)
        {
            PrimaryCard.CardMoved(card, target, region, origin);
        }

        if (ejectedCard != null)
        {
            ejectedCard.CardMoved(card, target, region, origin);
        }
    }

    public void CardGainedStatus(Card theCard, Card source, sbyte statusType)
    {
        if (PrimaryCard != null)
        {
            PrimaryCard.CardGainedStatus(theCard, source, statusType);
        }

        if (ejectedCard != null)
        {
            ejectedCard.CardGainedStatus(theCard, source, statusType);
        }
    }

    public void CardAttacked(Card attacker, Card target)
    {
        if (PrimaryCard != null)
        {
            PrimaryCard.CardAttacked(attacker, target);
        }

        if (ejectedCard != null)
        {
            ejectedCard.CardAttacked(attacker, target);
        }
    }

    public void CardCounterAttacked(Card attacker, Card target)
    {
        if (PrimaryCard != null)
        {
            PrimaryCard.CardCounterAttacked(attacker, target);
        }

        if (ejectedCard != null)
        {
            ejectedCard.CardCounterAttacked(attacker, target);
        }
    }

    public void CardDamaged(Card damagedCard, Card source)
    {
        if (PrimaryCard != null)
        {
            PrimaryCard.CardDamaged(damagedCard, source);
        }

        if (ejectedCard != null)
        {
            ejectedCard.CardDamaged(damagedCard, source);
        }
    }

    public void CardDied(Card deadCard, Card source)
    {
        if (PrimaryCard != null)
        {
            PrimaryCard.CardDied(deadCard, source);
        }

        if (ejectedCard != null)
        {
            ejectedCard.CardDied(deadCard, source);
        }
    }

    public void CardDrawn(Card drawnCard, bool regularDraw, bool isNewTurn)
    {
        if (PrimaryCard != null)
        {
            PrimaryCard.CardDrawn(drawnCard, regularDraw, isNewTurn);
        }

        if (ejectedCard != null)
        {
            ejectedCard.CardDrawn(drawnCard, regularDraw, isNewTurn);
        }
    }

    public void CardDiscardEffect(sbyte playerIndex, int numberOfCards)
    {
        if (PrimaryCard != null)
        {
            PrimaryCard.CardDiscardEffect(playerIndex, numberOfCards);
        }

        if (ejectedCard != null)
        {
            ejectedCard.CardDiscardEffect(playerIndex, numberOfCards);
        }
    }

    public void SecretTriggered(Card secret, Card source)
    {
        if (PrimaryCard != null)
        {
            PrimaryCard.SecretTriggered(secret, source);
        }

        if (ejectedCard != null)
        {
            ejectedCard.SecretTriggered(secret, source);
        }
    }

    public void SecretDestroyed(Card secret, Card source)
    {
        if (PrimaryCard != null)
        {
            PrimaryCard.SecretDestroyed(secret, source);
        }

        if (ejectedCard != null)
        {
            ejectedCard.SecretDestroyed(secret, source);
        }
    }

    public void TraitEffectActivating(BaseTraitEffect effect, Card source, CardStack target, RegionEnum region)
    {
        if (PrimaryCard != null)
        {
            PrimaryCard.TraitEffectActivating(effect, source, target, region);
        }

        if (ejectedCard != null)
        {
            ejectedCard.TraitEffectActivating(effect, source, target, region);
        }
    }

    public bool CheckDiscard(Player[] players)
    {
        if (PrimaryCard != null && PrimaryCard.CanDiscard())
        {
            PrimaryCard.Discard(players);
            PrimaryCard = null;
            if (ejectedCard != null)
            {
                PrimaryCard = ejectedCard;
                ejectedCard = null;
            }

            return true;
        }

        return false;
    }

    public void SetEjectedCard(Card card)
    {
        if (PrimaryCard != null && PrimaryCard.HasPilot())
        {
            UnitCard unitCard = (UnitCard) PrimaryCard;
            if (unitCard.EmbarkedPilot.EqualsTo(card))
            {
                unitCard.EmbarkedPilot.PilotEmbarked = false;
                ejectedCard = unitCard.EmbarkedPilot;
                unitCard.EmbarkedPilot = null;
            }
        }
    }

    public EntityCard GetEjectedCard()
    {
        return ejectedCard;
    }
}