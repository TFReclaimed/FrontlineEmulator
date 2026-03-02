using Frontline.Game.Card;

namespace Frontline.Battle.Traits;

public class ApplyDamageMultiply : ApplyDamage
{
    public required TraitTargeting CountInfo { get; set; }

    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        if (card.GetTemplate().Type == CardType.Secret)
        {
            GameState.SecretDestroyed(card, source);
            var list = GameState.FindCardStack(card);
            for (var i = 0; i < list.Count; i++)
            {
                var secrets = list[i].PrimaryCard!.GetSecrets();

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

        var num2 = CountInfo.CalculateCount(GameState, active);
        var currentHealth = card.GetCurrentHealth(false);
        var attack = (sbyte) (Damage * num2);
        var bypass = (sbyte) (BypassDefense * num2);
        if (Damage == -1)
        {
            attack = currentHealth;
        }
        else if (Damage > 0 && active.DataValue > 0)
        {
            attack = (sbyte) (active.DataValue * num2);
        }

        if (BypassDefense == -1)
        {
            bypass = currentHealth;
        }
        else if (BypassDefense > 0 && active.DataValue > 0)
        {
            bypass = (sbyte) (active.DataValue * num2);
        }

        if (currentHealth > 0)
        {
            card.TakeDamage(attack, bypass, source, true);
        }
    }
}