namespace Frontline.Battle.Traits;

public class StatTraitOverride : BaseTraitEffect
{
    public const sbyte fromTarget = 0;

    public const sbyte fromSource = 1;

    public const sbyte fromTraitTarget = 2;

    public bool attack;

    public bool bypassDefense;

    public bool defense;

    public bool health;

    public bool command;

    public int statSource;

    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        if (active.dataValue != 0)
        {
            return;
        }

        int dataValue = 0;
        Card card2 = card;
        if (statSource == 1)
        {
            card2 = source;
        }
        else if (statSource == 2)
        {
            int count = GameState.GetTemporaryEffects().Count;
            ActiveTrait activeTrait = null;
            for (int i = 0; i < count; i++)
            {
                activeTrait = GameState.GetTemporaryEffects()[i];
                if (activeTrait.GetTraitInfo().TargetTrait() && activeTrait.GetTraitSource().EqualsTo(source))
                {
                    card2 = activeTrait.GetTraitTarget();
                    break;
                }
            }
        }

        if (attack)
        {
            dataValue = card2.GetCurrentAttack(null, false);
        }
        else if (bypassDefense)
        {
            dataValue = card2.GetCurrentBypassDefense(null, false);
        }
        else if (defense)
        {
            dataValue = card2.GetCurrentDefense(false);
        }
        else if (health)
        {
            dataValue = card2.GetCurrentHealth(false);
        }
        else if (command)
        {
            dataValue = card2.GetCurrentCost();
        }

        active.dataValue = dataValue;
        GameState.CaptureTemporaryEffect(active);
        base.Apply(card, source, active);
    }

    public override int GetOverrideData(ActiveTrait active)
    {
        return active.dataValue;
    }
}