using Frontline.Game.Card;

namespace Frontline.Battle.Traits;

public class NegateActivationEffect : BaseTraitEffect
{
    public const sbyte Damage = 1;

    public const sbyte Heals = 2;

    public const sbyte Secret = 3;

    public sbyte EffectType { get; set; }

    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        base.Apply(card, source, active);
        GameState.GetBattleEffects().Add(active);
    }

    public override void Init(Card card, Card source, ActiveTrait active)
    {
        GameState.GetBattleEffects().Add(active);
    }

    public override void Deactivate(ActiveTrait active)
    {
        GameState.GetBattleEffects().Remove(active);
        base.Deactivate(active);
    }

    public override bool DoesNegateEffect(BaseTraitEffect effect, Card source, ActiveTrait active)
    {
        if (Deterable && active.Detered)
        {
            return false;
        }

        if (active.GetTraitSource().ActiveData.Owner == source.ActiveData.Owner)
        {
            return false;
        }

        switch (EffectType)
        {
            case Damage:
                if (effect.IsDamageHeal(true))
                {
                    return true;
                }

                break;
            case Heals:
                if (effect.IsDamageHeal(false))
                {
                    return true;
                }

                break;
            case Secret:
                if (source.GetTemplate().Type == CardType.Secret)
                {
                    return true;
                }

                break;
        }

        return false;
    }
}