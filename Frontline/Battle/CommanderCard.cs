using Frontline.Battle.CcgEvents;
using Frontline.Battle.Traits;
using Frontline.Data.Entities;
using Frontline.Game.Card;

namespace Frontline.Battle;

public class CommanderCard : Card
{
    public sbyte Defense { get; set; }

    public List<Card> Secrets { get; set; } = [];

    private Player _player = null!;

    public CommanderCard(CcgGameState game, CommanderCardTemplate template)
        : base(game, template)
    {
    }

    public CommanderCard(CcgGameState game, CommanderCard other)
        : base(game, other)
    {
        Secrets = other.Secrets;
        Defense = other.Defense;
    }

    public CommanderCard(CcgGameState game, CommanderCardTemplate template, ItemEntity itemEntity)
        : base(game, template, itemEntity)
    {
    }

    public override void Setup()
    {
        base.Setup();
        Secrets = [];
        Defense = 0;
    }

    public override List<Card> GetSecrets()
    {
        return Secrets;
    }

    public override void InitActiveData()
    {
        base.InitActiveData();

        foreach (var secret in Secrets)
        {
            secret.InitActiveData();
        }
    }

    public override void InitStackedCards()
    {
        for (var num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num] = Secrets[num].GenerateAndInit(GameState);
        }

        base.InitStackedCards();
    }

    public override Card? FindTraitActor(int cardId, sbyte ownerId)
    {
        var card = base.FindTraitActor(cardId, ownerId);
        if (card != null)
        {
            return card;
        }

        foreach (var secret in Secrets)
        {
            card = secret;
            if (card.InstanceId == cardId && card.ActiveData.Owner == ownerId)
            {
                return card;
            }
        }

        return null;
    }

    public override bool DoesMatchTargetingInfo(TraitTargeting info, Card source)
    {
        if (base.DoesMatchTargetingInfo(info, source))
        {
            return true;
        }

        foreach (var secret in Secrets)
        {
            if (secret.DoesMatchTargetingInfo(info, source))
            {
                return true;
            }
        }

        return false;
    }

    public void SetPlayer(Player player)
    {
        _player = player;
    }

    public override sbyte GetCurrentHealth(bool combatLog)
    {
        var health = _player.Resources.Health;
        List<EventLogTraitCardInfo> list = [];

        foreach (var activeTrait in ActiveData.ActiveTraits)
        {
            var bonus = activeTrait.GetTraitInfo().GetHealthBonus(activeTrait);
            if (bonus == 0)
            {
                continue;
            }

            if (combatLog)
            {
                var eventLogTraitCardInfo = new EventLogTraitCardInfo
                {
                    InstanceId = activeTrait.GetTraitSource().InstanceId,
                    Owner = activeTrait.GetTraitSource().ActiveData.Owner,
                    EffectId = activeTrait.GetTraitInfo().EffectTraitId,
                    TraitId = activeTrait.GetTraitInfo().TraitParentId,
                    Data = bonus
                };
                list.Add(eventLogTraitCardInfo);
            }

            health += bonus;
        }

        if (combatLog && list.Count > 0)
        {
            var count = list.Count;
            var combatBuffsEvent = new CombatBuffsCcgEvent(CcgEventType.CombatBuffsAttack, InstanceId,
                ActiveData.Owner, 0, 0);
            combatBuffsEvent.BuffTraits = new EventLogTraitCardInfo[count];
            for (var j = 0; j < count; j++)
            {
                combatBuffsEvent.BuffTraits[j] = list[j];
            }

            GameState.AddCcgEventLog(combatBuffsEvent);
        }

        return health;
    }

    public override sbyte GetCurrentDefense(bool combatLog)
    {
        var defense = Defense;
        List<EventLogTraitCardInfo> list = [];

        foreach (var activeTrait in ActiveData.ActiveTraits)
        {
            var bonus = activeTrait.GetTraitInfo().GetDefenseBonus(activeTrait);
            if (bonus == 0)
            {
                continue;
            }

            if (combatLog)
            {
                var eventLogTraitCardInfo = new EventLogTraitCardInfo();
                eventLogTraitCardInfo.InstanceId = activeTrait.GetTraitSource().InstanceId;
                eventLogTraitCardInfo.Owner = activeTrait.GetTraitSource().ActiveData.Owner;
                eventLogTraitCardInfo.EffectId = activeTrait.GetTraitInfo().EffectTraitId;
                eventLogTraitCardInfo.TraitId = activeTrait.GetTraitInfo().TraitParentId;
                eventLogTraitCardInfo.Data = bonus;
                list.Add(eventLogTraitCardInfo);
            }

            defense += bonus;
        }

        if (combatLog && list.Count > 0)
        {
            var count = list.Count;
            var combatBuffsEvent = new CombatBuffsCcgEvent(CcgEventType.CombatBuffsAttack, InstanceId,
                ActiveData.Owner, 0, 0);
            combatBuffsEvent.BuffTraits = new EventLogTraitCardInfo[count];
            for (var j = 0; j < count; j++)
            {
                combatBuffsEvent.BuffTraits[j] = list[j];
            }

            GameState.AddCcgEventLog(combatBuffsEvent);
        }

        return defense;
    }

    public override void TakeDamage(sbyte attack, sbyte bypass, Card source, bool checkDeath)
    {
        var attackDamage = attack;
        var bypass2 = bypass;
        foreach (var activeTrait in ActiveData.ActiveTraits)
        {
            if (activeTrait.GetTraitInfo().IsDamageImmunity(false, activeTrait))
            {
                attackDamage = 0;
            }

            if (activeTrait.GetTraitInfo().IsDamageImmunity(true, activeTrait))
            {
                bypass2 = 0;
            }
        }

        var currentDefense = GetCurrentDefense(false);
        if (currentDefense < 0)
        {
            currentDefense = 0;
        }

        var b3 = attackDamage < currentDefense ? attackDamage : currentDefense;
        Defense -= b3;
        attackDamage -= b3;
        _player.TakeDamage(attackDamage, bypass2, source);
    }

    public override sbyte HealDamage(CardStack? stack, sbyte heal)
    {
        return _player.Resources.HealDamage(heal);
    }

    public override sbyte GetMaxModHealth()
    {
        var health = _player.Resources.MaxHealth;
        foreach (var activeTrait in ActiveData.ActiveTraits)
        {
            var bonus = activeTrait.GetTraitInfo().GetHealthBonus(activeTrait);
            if (bonus != 0)
            {
                health += bonus;
            }
        }

        return health;
    }

    public override void CardDeployed(Card deployed)
    {
        base.CardDeployed(deployed);
        for (var num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].CardDeployed(deployed);
        }
    }

    public override void NewTurn(sbyte playerIndex)
    {
        base.NewTurn(playerIndex);
        for (var num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].NewTurn(playerIndex);
        }
    }

    public override void EndTurn(sbyte playerIndex)
    {
        base.EndTurn(playerIndex);
        Defense = 0;
        for (var num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].EndTurn(playerIndex);
        }
    }

    public override void CardMoved(Card card, CardStack target, Region region, Region origin)
    {
        base.CardMoved(card, target, region, origin);
        for (var num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].CardMoved(card, target, region, origin);
        }
    }

    public override void CardAttacked(Card attacker, Card target)
    {
        base.CardAttacked(attacker, target);
        for (var num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].CardAttacked(attacker, target);
        }
    }

    public override void CardCounterAttacked(Card attacker, Card target)
    {
        base.CardCounterAttacked(attacker, target);
        for (var num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].CardCounterAttacked(attacker, target);
        }
    }

    public override void CardGainedStatus(Card theCard, Card source, ApplyStatusTraitStatusType statusType)
    {
        base.CardGainedStatus(theCard, source, statusType);
        for (var num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].CardGainedStatus(theCard, source, statusType);
        }
    }

    public override void CardDamaged(Card damagedCard, Card source)
    {
        base.CardDamaged(damagedCard, source);
        for (var num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].CardDamaged(damagedCard, source);
        }
    }

    public override void CardDied(Card deadCard, Card source)
    {
        base.CardDied(deadCard, source);
        for (var num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].CardDied(deadCard, source);
        }
    }

    public override void CardDrawn(Card drawnCard, bool regularDraw, bool isNewTurn)
    {
        base.CardDrawn(drawnCard, regularDraw, isNewTurn);
        for (var num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].CardDrawn(drawnCard, regularDraw, isNewTurn);
        }
    }

    public override void CardDiscardEffect(sbyte playerIndex, int numberOfCards)
    {
        base.CardDiscardEffect(playerIndex, numberOfCards);
        for (var num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].CardDiscardEffect(playerIndex, numberOfCards);
        }
    }

    public override void SecretTriggered(Card secret, Card? source)
    {
        base.SecretTriggered(secret, source);
        for (var num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].SecretTriggered(secret, source);
        }
    }

    public override void SecretDestroyed(Card secret, Card source)
    {
        base.SecretDestroyed(secret, source);
        for (var num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].SecretDestroyed(secret, source);
        }
    }

    public override void TraitEffectActivating(BaseTraitEffect effect, Card source, CardStack? target, Region region)
    {
        base.TraitEffectActivating(effect, source, target, region);
        for (var num = Secrets.Count - 1; num >= 0; num--)
        {
            Secrets[num].TraitEffectActivating(effect, source, target, region);
        }
    }
}