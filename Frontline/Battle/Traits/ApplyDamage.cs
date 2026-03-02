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
            var list = GameState.FindCardStack(card);
            foreach (var cardStack in list)
            {
                var secrets = cardStack.PrimaryCard!.GetSecrets();

                for (var num = secrets.Count - 1; num >= 0; num--)
                {
                    if (secrets[num].EqualsTo(card))
                    {
                        secrets.RemoveAt(num);
                    }
                }
            }

            card.Discard(GameState.Players);
            return;
        }

        if (card.GetTemplate().Type == CardType.Pilot)
        {
            var unitCard = (UnitCard) card;
            if (unitCard.IsEmbarked())
            {
                return;
            }
        }

        var currentHealth = card.GetCurrentHealth(false);
        var attack = Damage;
        var bypass = BypassDefense;
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
        if (owner.IsCardTraitsDetered() || DurationData.Type != TraitDurationType.Permanent ||
            owner.ActiveData.Owner != playerIndex)
        {
            return;
        }

        var region = Region.NumRegions;
        var target = GameState.FindCardStack(owner)[0];
        if (Targets.Area == TargetableArea.CurrentRegion)
        {
            region = GameState.GetTraitActorRegion(playerIndex, owner.InstanceId);
        }

        Activate(owner, target, region);
    }
}