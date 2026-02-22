using Frontline.Game.Card;

namespace Frontline.Battle.Traits;

public class ApplyDamage : BaseTraitEffect
{
    public sbyte Damage { get; set; }

    public sbyte BypassDefense { get; set; }

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
                list2 = list[i].PrimaryCard.GetSecrets();
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

            card.Discard(GameState.Players);
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
        sbyte attack = Damage;
        sbyte bypass = BypassDefense;
        if (Damage == -1)
        {
            attack = currentHealth;
        }
        else if (Damage > 0 && active.DataValue > 0)
        {
            attack = (sbyte) active.DataValue;
        }

        if (BypassDefense == -1)
        {
            bypass = currentHealth;
        }
        else if (BypassDefense > 0 && active.DataValue > 0)
        {
            bypass = (sbyte) active.DataValue;
        }

        if (currentHealth > 0)
        {
            card.TakeDamage(attack, bypass, source, true);
        }
    }

    public override void OnNewTurnEvent(Card owner, sbyte playerIndex)
    {
        if (!owner.IsCardTraitsDetered() && DurationData.Type == TraitDurationType.Permanent &&
            owner.ActiveData.Owner == playerIndex)
        {
            RegionEnum region = RegionEnum.NumRegions;
            CardStack target = GameState.FindCardStack(owner)[0];
            if (Targets.Area == TargetableArea.CurrentRegion)
            {
                region = GameState.GetTraitActorRegion(playerIndex, owner.InstanceId);
            }

            Activate(owner, target, region);
        }
    }
}