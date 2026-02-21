using Frontline.Game.Card;

namespace Frontline.Battle.Traits;

public class ApplyDamage : BaseTraitEffect
{
    public sbyte damage;

    public sbyte bypassDefense;

    public override bool IsDamageHeal(bool damage)
    {
        return damage;
    }

    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        if (card.GetTemplate().Type == CardType.Secret)
        {
            GameState.SecretDestroyed(card, source);
            List<CardStack> list = GameState.FindCardStack(card);
            List<Card> list2 = null;
            for (int i = 0; i < list.Count; i++)
            {
                list2 = list[i].primaryCard.GetSecrets();
                if (list2 == null)
                {
                    continue;
                }

                for (int num = list2.Count - 1; num >= 0; num--)
                {
                    if (list2[num].EqualsTo(card))
                    {
                        list2.RemoveAt(num);
                    }
                }
            }

            card.Discard(GameState.players);
            return;
        }

        if (card.GetTemplate().Type == CardType.Pilot)
        {
            UnitCard unitCard = (UnitCard) card;
            if (unitCard != null && unitCard.IsEmbarked())
            {
                return;
            }
        }

        sbyte currentHealth = card.GetCurrentHealth(false);
        sbyte attack = damage;
        sbyte bypass = bypassDefense;
        if (damage == -1)
        {
            attack = currentHealth;
        }
        else if (damage > 0 && active.dataValue > 0)
        {
            attack = (sbyte) active.dataValue;
        }

        if (bypassDefense == -1)
        {
            bypass = currentHealth;
        }
        else if (bypassDefense > 0 && active.dataValue > 0)
        {
            bypass = (sbyte) active.dataValue;
        }

        if (currentHealth > 0)
        {
            card.TakeDamage(attack, bypass, source, true);
        }
    }

    public override void OnNewTurnEvent(Card owner, sbyte playerIndex)
    {
        if (!owner.IsCardTraitsDetered() && durationData.type == TraitDurationType.Permanent &&
            owner.activeData.owner == playerIndex)
        {
            RegionEnum region = RegionEnum.NumRegions;
            CardStack target = GameState.FindCardStack(owner)[0];
            if (targets.area == TargetableArea.CurrentRegion)
            {
                region = GameState.GetTraitActorRegion(playerIndex, owner.instanceId);
            }

            Activate(owner, target, region);
        }
    }
}