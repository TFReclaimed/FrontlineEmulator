using Frontline.Battle.CcgEvents;
using Frontline.Game.Card;

namespace Frontline.Battle;

public class CommanderCard : Card
{
    public sbyte defense;

    public List<Card> secrets;

    private Player parent;

    public CommanderCard(CCG game)
        : base(game)
    {
    }

    public CommanderCard(CCG game, Card other)
        : base(game, other)
    {
        if (other is CommanderCard)
        {
            CommanderCard commanderCard = (CommanderCard) other;
            secrets = commanderCard.secrets;
            defense = commanderCard.defense;
        }
    }

    public override void Setup()
    {
        base.Setup();
        CommanderCardTemplate commanderTemplate = (CommanderCardTemplate) GetTemplate();
        secrets = new List<Card>();
        defense = 0;
    }

    public override List<Card> GetSecrets()
    {
        return secrets;
    }

    public override void InitActiveData()
    {
        if (activeData != null)
        {
            base.InitActiveData();
        }

        if (secrets == null)
        {
            secrets = new List<Card>();
            return;
        }

        for (int i = 0; i < secrets.Count; i++)
        {
            secrets[i].InitActiveData();
        }
    }

    public override void InitStackedCards()
    {
        if (secrets != null)
        {
            for (int num = secrets.Count - 1; num >= 0; num--)
            {
                secrets[num] = secrets[num].GenerateAndInit(GameState);
            }
        }

        base.InitStackedCards();
    }

    public override Card FindTraitActor(int cardId, sbyte ownerId)
    {
        Card card = base.FindTraitActor(cardId, ownerId);
        if (card != null)
        {
            return card;
        }

        for (int i = 0; i < secrets.Count; i++)
        {
            card = secrets[i];
            if (card.instanceId == cardId && card.activeData.owner == ownerId)
            {
                return card;
            }
        }

        return null;
    }

    public override bool CanDeployAnywhere()
    {
        return false;
    }

    public override bool DoesMatchTargetingInfo(TraitTargeting info, Card source)
    {
        if (base.DoesMatchTargetingInfo(info, source))
        {
            return true;
        }

        for (int i = 0; i < secrets.Count; i++)
        {
            if (secrets[i].DoesMatchTargetingInfo(info, source))
            {
                return true;
            }
        }

        return false;
    }

    public void SetPlayer(Player player)
    {
        parent = player;
    }

    public override sbyte GetCurrentHealth(bool combatLog)
    {
        if (parent == null || parent.resources == null)
        {
            return 0;
        }

        sbyte b = parent.resources.health;
        sbyte b2 = 0;
        ActiveTrait activeTrait = null;
        EventLogTraitCardInfo eventLogTraitCardInfo = null;
        List<EventLogTraitCardInfo> list = null;
        if (combatLog)
        {
            list = new List<EventLogTraitCardInfo>();
        }

        for (int i = 0; i < activeData.activeTraits.Count; i++)
        {
            activeTrait = activeData.activeTraits[i];
            b2 = activeTrait.GetTraitInfo().GetHealthBonus(activeTrait);
            if (b2 != 0)
            {
                if (combatLog)
                {
                    eventLogTraitCardInfo = new EventLogTraitCardInfo();
                    eventLogTraitCardInfo.instanceId = activeTrait.GetTraitSource().instanceId;
                    eventLogTraitCardInfo.owner = activeTrait.GetTraitSource().activeData.owner;
                    eventLogTraitCardInfo.effectID = activeTrait.GetTraitInfo().effectTraitID;
                    eventLogTraitCardInfo.traitID = activeTrait.GetTraitInfo().traitParentID;
                    eventLogTraitCardInfo.data = b2;
                    list.Add(eventLogTraitCardInfo);
                }

                b += b2;
            }
        }

        if (combatLog && list.Count > 0)
        {
            int count = list.Count;
            CombatBuffsCCGEvent combatBuffsCCGEvent =
                new CombatBuffsCCGEvent(CCGEventType.CombatBuffsAttack, instanceId, activeData.owner, 0, 0);
            combatBuffsCCGEvent.buffTraits = new EventLogTraitCardInfo[count];
            for (int j = 0; j < count; j++)
            {
                combatBuffsCCGEvent.buffTraits[j] = list[j];
            }

            GameState.AddCCGEventLog(combatBuffsCCGEvent);
        }

        return b;
    }

    public override sbyte GetCurrentDefense(bool combatLog)
    {
        sbyte b = defense;
        sbyte b2 = 0;
        ActiveTrait activeTrait = null;
        EventLogTraitCardInfo eventLogTraitCardInfo = null;
        List<EventLogTraitCardInfo> list = null;
        if (combatLog)
        {
            list = new List<EventLogTraitCardInfo>();
        }

        for (int i = 0; i < activeData.activeTraits.Count; i++)
        {
            activeTrait = activeData.activeTraits[i];
            b2 = activeTrait.GetTraitInfo().GetDefenseBonus(activeTrait);
            if (b2 != 0)
            {
                if (combatLog)
                {
                    eventLogTraitCardInfo = new EventLogTraitCardInfo();
                    eventLogTraitCardInfo.instanceId = activeTrait.GetTraitSource().instanceId;
                    eventLogTraitCardInfo.owner = activeTrait.GetTraitSource().activeData.owner;
                    eventLogTraitCardInfo.effectID = activeTrait.GetTraitInfo().effectTraitID;
                    eventLogTraitCardInfo.traitID = activeTrait.GetTraitInfo().traitParentID;
                    eventLogTraitCardInfo.data = b2;
                    list.Add(eventLogTraitCardInfo);
                }

                b += b2;
            }
        }

        if (combatLog && list.Count > 0)
        {
            int count = list.Count;
            CombatBuffsCCGEvent combatBuffsCCGEvent =
                new CombatBuffsCCGEvent(CCGEventType.CombatBuffsAttack, instanceId, activeData.owner, 0, 0);
            combatBuffsCCGEvent.buffTraits = new EventLogTraitCardInfo[count];
            for (int j = 0; j < count; j++)
            {
                combatBuffsCCGEvent.buffTraits[j] = list[j];
            }

            GameState.AddCCGEventLog(combatBuffsCCGEvent);
        }

        return b;
    }

    public override void TakeDamage(sbyte attack, sbyte bypass, Card source, bool checkDeath)
    {
        sbyte b = attack;
        sbyte bypass2 = bypass;
        ActiveTrait activeTrait = null;
        for (int i = 0; i < activeData.activeTraits.Count; i++)
        {
            activeTrait = activeData.activeTraits[i];
            if (activeTrait.GetTraitInfo().IsDamageImmunity(false, activeTrait))
            {
                b = 0;
            }

            if (activeTrait.GetTraitInfo().IsDamageImmunity(true, activeTrait))
            {
                bypass2 = 0;
            }
        }

        sbyte b2 = GetCurrentDefense(false);
        if (b2 < 0)
        {
            b2 = 0;
        }

        sbyte b3 = ((b < b2) ? b : b2);
        defense -= b3;
        b -= b3;
        parent.TakeDamage(b, bypass2, source);
    }

    public override sbyte HealDamage(CardStack stack, sbyte heal)
    {
        return parent.resources.HealDamage(heal);
    }

    public sbyte GetResetDefense()
    {
        sbyte b = 0;
        ActiveTrait activeTrait = null;
        for (int i = 0; i < activeData.activeTraits.Count; i++)
        {
            activeTrait = activeData.activeTraits[i];
            b += activeTrait.GetTraitInfo().GetDefenseBonus(activeTrait);
        }

        return b;
    }

    public sbyte GetMaxDefense()
    {
        return 0;
    }

    public sbyte GetMaxHealth()
    {
        if (parent == null || parent.resources == null)
        {
            return 0;
        }

        return parent.resources.maxHealth;
    }

    public override sbyte GetMaxModHealth()
    {
        if (parent == null || parent.resources == null)
        {
            return 0;
        }

        sbyte b = parent.resources.maxHealth;
        sbyte b2 = 0;
        ActiveTrait activeTrait = null;
        for (int i = 0; i < activeData.activeTraits.Count; i++)
        {
            activeTrait = activeData.activeTraits[i];
            b2 = activeTrait.GetTraitInfo().GetHealthBonus(activeTrait);
            if (b2 != 0)
            {
                b += b2;
            }
        }

        return b;
    }

    public override void CardDeployed(Card deployed)
    {
        base.CardDeployed(deployed);
        for (int num = secrets.Count - 1; num >= 0; num--)
        {
            secrets[num].CardDeployed(deployed);
        }
    }

    public override void NewTurn(sbyte playerIndex)
    {
        base.NewTurn(playerIndex);
        for (int num = secrets.Count - 1; num >= 0; num--)
        {
            secrets[num].NewTurn(playerIndex);
        }
    }

    public override void EndTurn(sbyte playerIndex)
    {
        base.EndTurn(playerIndex);
        defense = 0;
        for (int num = secrets.Count - 1; num >= 0; num--)
        {
            secrets[num].EndTurn(playerIndex);
        }
    }

    public override void CardMoved(Card card, CardStack target, RegionEnum region, RegionEnum origin)
    {
        base.CardMoved(card, target, region, origin);
        for (int num = secrets.Count - 1; num >= 0; num--)
        {
            secrets[num].CardMoved(card, target, region, origin);
        }
    }

    public override void CardAttacked(Card attacker, Card target)
    {
        base.CardAttacked(attacker, target);
        for (int num = secrets.Count - 1; num >= 0; num--)
        {
            secrets[num].CardAttacked(attacker, target);
        }
    }

    public override void CardCounterAttacked(Card attacker, Card target)
    {
        base.CardCounterAttacked(attacker, target);
        for (int num = secrets.Count - 1; num >= 0; num--)
        {
            secrets[num].CardCounterAttacked(attacker, target);
        }
    }

    public override void CardGainedStatus(Card theCard, Card source, sbyte statusType)
    {
        base.CardGainedStatus(theCard, source, statusType);
        for (int num = secrets.Count - 1; num >= 0; num--)
        {
            secrets[num].CardGainedStatus(theCard, source, statusType);
        }
    }

    public override void CardDamaged(Card damagedCard, Card source)
    {
        base.CardDamaged(damagedCard, source);
        for (int num = secrets.Count - 1; num >= 0; num--)
        {
            secrets[num].CardDamaged(damagedCard, source);
        }
    }

    public override void CardDied(Card deadCard, Card source)
    {
        base.CardDied(deadCard, source);
        for (int num = secrets.Count - 1; num >= 0; num--)
        {
            secrets[num].CardDied(deadCard, source);
        }
    }

    public override void CardDrawn(Card drawnCard, bool regularDraw, bool isNewTurn)
    {
        base.CardDrawn(drawnCard, regularDraw, isNewTurn);
        for (int num = secrets.Count - 1; num >= 0; num--)
        {
            secrets[num].CardDrawn(drawnCard, regularDraw, isNewTurn);
        }
    }

    public override void CardDiscardEffect(sbyte playerIndex, int numberOfCards)
    {
        base.CardDiscardEffect(playerIndex, numberOfCards);
        for (int num = secrets.Count - 1; num >= 0; num--)
        {
            secrets[num].CardDiscardEffect(playerIndex, numberOfCards);
        }
    }

    public override void SecretTriggered(Card secret, Card source)
    {
        base.SecretTriggered(secret, source);
        for (int num = secrets.Count - 1; num >= 0; num--)
        {
            secrets[num].SecretTriggered(secret, source);
        }
    }

    public override void SecretDestroyed(Card secret, Card source)
    {
        base.SecretDestroyed(secret, source);
        for (int num = secrets.Count - 1; num >= 0; num--)
        {
            secrets[num].SecretDestroyed(secret, source);
        }
    }

    public override void TraitEffectActivating(BaseTraitEffect effect, Card source, CardStack target, RegionEnum region)
    {
        base.TraitEffectActivating(effect, source, target, region);
        for (int num = secrets.Count - 1; num >= 0; num--)
        {
            secrets[num].TraitEffectActivating(effect, source, target, region);
        }
    }
}