using System.Text.Json.Serialization;

namespace Frontline.Battle.Traits;

public class StatTraitOverride : BaseTraitEffect
{
    public const sbyte fromTarget = 0;

    public const sbyte fromSource = 1;

    public const sbyte fromTraitTarget = 2;

    [JsonPropertyName("attack")]
    public bool IsAttack { get; set; }

    public bool BypassDefense { get; set; }

    public bool Defense { get; set; }

    public bool Health { get; set; }

    public bool Command { get; set; }

    public int StatSource { get; set; }

    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        if (active.DataValue != 0)
        {
            return;
        }

        int dataValue = 0;
        Card card2 = card;
        if (StatSource == 1)
        {
            card2 = source;
        }
        else if (StatSource == 2)
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

        if (IsAttack)
        {
            dataValue = card2.GetCurrentAttack(null, false);
        }
        else if (BypassDefense)
        {
            dataValue = card2.GetCurrentBypassDefense(null, false);
        }
        else if (Defense)
        {
            dataValue = card2.GetCurrentDefense(false);
        }
        else if (Health)
        {
            dataValue = card2.GetCurrentHealth(false);
        }
        else if (Command)
        {
            dataValue = card2.GetCurrentCost();
        }

        active.DataValue = dataValue;
        GameState.CaptureTemporaryEffect(active);
        base.Apply(card, source, active);
    }

    public override int GetOverrideData(ActiveTrait active)
    {
        return active.DataValue;
    }
}