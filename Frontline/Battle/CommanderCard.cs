using Frontline.Battle.CcgEvents;
using Frontline.Data.Entities;
using Frontline.Game.Card;

namespace Frontline.Battle;

public class CommanderCard : Card
{
    public sbyte Defense { get; set; }

    public List<Card> Secrets { get; set; }

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
            Secrets = commanderCard.Secrets;
            Defense = commanderCard.Defense;
        }
    }

    public CommanderCard(CCG game, ItemEntity itemEntity)
        : base(game, itemEntity)
    {
    }

    public override void Setup()
    {
        base.Setup();
        CommanderCardTemplate commanderTemplate = (CommanderCardTemplate) GetTemplate();
        Secrets = new List<Card>();
        Defense = 0;
    }

    public override List<Card> GetSecrets()
    {
        return Secrets;
    }

    public override void InitActiveData()
    {
        if (ActiveData != null)
        {
            base.InitActiveData();
        }

        if (Secrets == null)
        {
            Secrets = new List<Card>();
            return;
        }

        for (int i = 0; i < Secrets.Count; i++)
        {
            Secrets[i].InitActiveData();
        }
    }

    public override void InitStackedCards()
    {
        if (Secrets != null)
        {
            for (int num = Secrets.Count - 1; num >= 0; num--)
            {
                Secrets[num] = Secrets[num].GenerateAndInit(GameState);
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

        for (int i = 0; i < Secrets.Count; i++)
        {
            card = Secrets[i];
            if (card.InstanceId == cardId && card.ActiveData.Owner == ownerId)
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

        for (int i = 0; i < Secrets.Count; i++)
        {
            if (Secrets[i].DoesMatchTargetingInfo(info, source))
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
        if (parent == null || parent.Resources == null)
        {
            return 0;
        }

        sbyte b = parent.Resources.Health;
        sbyte b2 = 0;
        ActiveTrait activeTrait = null;
        EventLogTraitCardInfo eventLogTraitCardInfo = null;
        List<EventLogTraitCardInfo> list = null;
        if (combatLog)
        {
            list = new List<EventLogTraitCardInfo>();
        }

        for (int i = 0; i < ActiveData.ActiveTraits.Count; i++)
        {
            activeTrait = ActiveData.ActiveTraits[i];
            b2 = activeTrait.GetTraitInfo().GetHealthBonus(activeTrait);
            if (b2 != 0)
            {
                if (combatLog)
                {
                    eventLogTraitCardInfo = new EventLogTraitCardInfo();
                    eventLogTraitCardInfo.InstanceId = activeTrait.GetTraitSource().InstanceId;
                    eventLogTraitCardInfo.Owner = activeTrait.GetTraitSource().ActiveData.Owner;
                    eventLogTraitCardInfo.EffectId = activeTrait.GetTraitInfo().EffectTraitId;
                    eventLogTraitCardInfo.TraitId = activeTrait.GetTraitInfo().TraitParentId;
                    eventLogTraitCardInfo.Data = b2;
                    list.Add(eventLogTraitCardInfo);
                }

                b += b2;
            }
        }

        if (combatLog && list.Count > 0)
        {
            int count = list.Count;
            CombatBuffsCCGEvent combatBuffsCCGEvent =
                new CombatBuffsCCGEvent(CCGEventType.CombatBuffsAttack, InstanceId, ActiveData.Owner, 0, 0);
            combatBuffsCCGEvent.BuffTraits = new EventLogTraitCardInfo[count];
            for (int j = 0; j < count; j++)
            {
                combatBuffsCCGEvent.BuffTraits[j] = list[j];
            }

            GameState.AddCCGEventLog(combatBuffsCCGEvent);
        }

        return b;
    }

    public override sbyte GetCurrentDefense(bool combatLog)
    {
        sbyte b = Defense;
        sbyte b2 = 0;
        ActiveTrait activeTrait = null;
        EventLogTraitCardInfo eventLogTraitCardInfo = null;
        List<EventLogTraitCardInfo> list = null;
        if (combatLog)
        {
            list = new List<EventLogTraitCardInfo>();
        }

        for (int i = 0; i < ActiveData.ActiveTraits.Count; i++)
        {
            activeTrait = ActiveData.ActiveTraits[i];
            b2 = activeTrait.GetTraitInfo().GetDefenseBonus(activeTrait);
            if (b2 != 0)
            {
                if (combatLog)
                {
                    eventLogTraitCardInfo = new EventLogTraitCardInfo();
                    eventLogTraitCardInfo.InstanceId = activeTrait.GetTraitSource().InstanceId;
                    eventLogTraitCardInfo.Owner = activeTrait.GetTraitSource().ActiveData.Owner;
                    eventLogTraitCardInfo.EffectId = activeTrait.GetTraitInfo().EffectTraitId;
                    eventLogTraitCardInfo.TraitId = activeTrait.GetTraitInfo().TraitParentId;
                    eventLogTraitCardInfo.Data = b2;
                    list.Add(eventLogTraitCardInfo);
                }

                b += b2;
            }
        }

        if (combatLog && list.Count > 0)
        {
            int count = list.Count;
            CombatBuffsCCGEvent combatBuffsCCGEvent =
                new CombatBuffsCCGEvent(CCGEventType.CombatBuffsAttack, InstanceId, ActiveData.Owner, 0, 0);
            combatBuffsCCGEvent.BuffTraits = new EventLogTraitCardInfo[count];
            for (int j = 0; j < count; j++)
            {
                combatBuffsCCGEvent.BuffTraits[j] = list[j];
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
        for (int i = 0; i < ActiveData.ActiveTraits.Count; i++)
        {
            activeTrait = ActiveData.ActiveTraits[i];
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
        Defense -= b3;
        b -= b3;
        parent.TakeDamage(b, bypass2, source);
    }

    public override sbyte HealDamage(CardStack stack, sbyte heal)
    {
        return parent.Resources.HealDamage(heal);
    }

    public sbyte GetResetDefense()
    {
        sbyte b = 0;
        ActiveTrait activeTrait = null;
        for (int i = 0; i < ActiveData.ActiveTraits.Count; i++)
        {
            activeTrait = ActiveData.ActiveTraits[i];
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
        if (parent == null || parent.Resources == null)
        {
            return 0;
        }

        return parent.Resources.MaxHealth;
    }

    public override sbyte GetMaxModHealth()
    {
        if (parent == null || parent.Resources == null)
        {
            return 0;
        }

        sbyte b = parent.Resources.MaxHealth;
        sbyte b2 = 0;
        ActiveTrait activeTrait = null;
        for (int i = 0; i < ActiveData.ActiveTraits.Count; i++)
        {
            activeTrait = ActiveData.ActiveTraits[i];
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
        for (int num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].CardDeployed(deployed);
        }
    }

    public override void NewTurn(sbyte playerIndex)
    {
        base.NewTurn(playerIndex);
        for (int num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].NewTurn(playerIndex);
        }
    }

    public override void EndTurn(sbyte playerIndex)
    {
        base.EndTurn(playerIndex);
        Defense = 0;
        for (int num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].EndTurn(playerIndex);
        }
    }

    public override void CardMoved(Card card, CardStack target, RegionEnum region, RegionEnum origin)
    {
        base.CardMoved(card, target, region, origin);
        for (int num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].CardMoved(card, target, region, origin);
        }
    }

    public override void CardAttacked(Card attacker, Card target)
    {
        base.CardAttacked(attacker, target);
        for (int num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].CardAttacked(attacker, target);
        }
    }

    public override void CardCounterAttacked(Card attacker, Card target)
    {
        base.CardCounterAttacked(attacker, target);
        for (int num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].CardCounterAttacked(attacker, target);
        }
    }

    public override void CardGainedStatus(Card theCard, Card source, sbyte statusType)
    {
        base.CardGainedStatus(theCard, source, statusType);
        for (int num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].CardGainedStatus(theCard, source, statusType);
        }
    }

    public override void CardDamaged(Card damagedCard, Card source)
    {
        base.CardDamaged(damagedCard, source);
        for (int num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].CardDamaged(damagedCard, source);
        }
    }

    public override void CardDied(Card deadCard, Card source)
    {
        base.CardDied(deadCard, source);
        for (int num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].CardDied(deadCard, source);
        }
    }

    public override void CardDrawn(Card drawnCard, bool regularDraw, bool isNewTurn)
    {
        base.CardDrawn(drawnCard, regularDraw, isNewTurn);
        for (int num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].CardDrawn(drawnCard, regularDraw, isNewTurn);
        }
    }

    public override void CardDiscardEffect(sbyte playerIndex, int numberOfCards)
    {
        base.CardDiscardEffect(playerIndex, numberOfCards);
        for (int num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].CardDiscardEffect(playerIndex, numberOfCards);
        }
    }

    public override void SecretTriggered(Card secret, Card source)
    {
        base.SecretTriggered(secret, source);
        for (int num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].SecretTriggered(secret, source);
        }
    }

    public override void SecretDestroyed(Card secret, Card source)
    {
        base.SecretDestroyed(secret, source);
        for (int num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].SecretDestroyed(secret, source);
        }
    }

    public override void TraitEffectActivating(BaseTraitEffect effect, Card source, CardStack target, RegionEnum region)
    {
        base.TraitEffectActivating(effect, source, target, region);
        for (int num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].TraitEffectActivating(effect, source, target, region);
        }
    }
}