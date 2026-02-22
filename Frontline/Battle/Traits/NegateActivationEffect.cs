using Frontline.Game.Card;

namespace Frontline.Battle.Traits;

public class NegateActivationEffect : BaseTraitEffect
{
    public const sbyte damage = 1;

    public const sbyte heals = 2;

    public const sbyte secret = 3;

    public const sbyte hacks = 4;

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
            case 1:
                if (effect.IsDamageHeal(true))
                {
                    return true;
                }

                break;
            case 2:
                if (effect.IsDamageHeal(false))
                {
                    return true;
                }

                break;
            case 3:
                if (source.GetTemplate().Type == CardType.Secret)
                {
                    return true;
                }

                break;
        }

        return false;
    }
}