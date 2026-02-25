using System.Text.Json.Serialization;

namespace Frontline.Battle.Traits;

public class ActedModExlusive : BaseTraitEffect
{
    public bool Deploy { get; set; }

    [JsonPropertyName("attack")]
    public bool IsAttack { get; set; }

    [JsonPropertyName("move")]
    public bool IsMove { get; set; }

    [JsonPropertyName("activate")]
    public bool IsActivate { get; set; }

    public override void Move(CardStack location, Region region, bool embark, ActiveTrait active)
    {
        if ((!Deterable || !active.Detered) && (DurationData.Charges <= 0 || active.DurationData.Charges != 0) &&
            IsMove)
        {
            var entityCard = (EntityCard) active.GetTraitTarget();
            entityCard.ClearActed(10);
            if (active.HasCharges())
            {
                active.ExpendCharge(GameState);
            }
        }
    }

    public override void Attack(Card target, ActiveTrait active)
    {
        if ((!Deterable || !active.Detered) && (DurationData.Charges <= 0 || active.DurationData.Charges != 0) &&
            IsAttack)
        {
            var entityCard = (EntityCard) active.GetTraitTarget();
            entityCard.ClearActed(12);
            if (active.HasCharges())
            {
                active.ExpendCharge(GameState);
            }
        }
    }

    public override void ActivateAction(CardStack location, Region region, ActiveTrait active)
    {
        if ((!Deterable || !active.Detered) && (DurationData.Charges <= 0 || active.DurationData.Charges != 0) &&
            IsActivate)
        {
            var entityCard = (EntityCard) active.GetTraitTarget();
            entityCard.ClearActed(6);
            if (active.HasCharges())
            {
                active.ExpendCharge(GameState);
            }
        }
    }
}