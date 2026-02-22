using System.Text.Json.Serialization;

namespace Frontline.Battle.Traits;

public class ActedModPassive : BaseTraitEffect
{
    public bool Deploy { get; set; }

    [JsonPropertyName("attack")]
    public bool IsAttack { get; set; }

    [JsonPropertyName("move")]
    public bool IsMove { get; set; }

    [JsonPropertyName("activate")]
    public bool IsActivate { get; set; }

    public override void Apply(Card card, Card source, ActiveTrait active)
    {
        sbyte b = 0;
        EntityCard entityCard = (EntityCard) card;
        bool flag = entityCard.IsCardTraitsDetered();
        if (!active.GetTraitInfo().Deterable)
        {
            flag = false;
        }

        if (active.HasCharges() || active.HasDuration())
        {
            base.Apply(card, source, active);
            if (Deploy && !flag)
            {
                if (Deploy)
                {
                    b = (sbyte) ((byte) b | 1);
                }

                if (IsAttack)
                {
                    b = (sbyte) ((byte) b | 2);
                }

                if (IsMove)
                {
                    b = (sbyte) ((byte) b | 4);
                }

                if (IsActivate)
                {
                    b = (sbyte) ((byte) b | 8);
                }

                entityCard.ClearActed(b);
            }
        }
        else if (!flag)
        {
            if (Deploy)
            {
                b = (sbyte) ((byte) b | 1);
            }

            if (IsAttack)
            {
                b = (sbyte) ((byte) b | 2);
            }

            if (IsMove)
            {
                b = (sbyte) ((byte) b | 4);
            }

            if (IsActivate)
            {
                b = (sbyte) ((byte) b | 8);
            }

            entityCard.ClearActed(b);
        }
    }

    public override void Move(CardStack location, RegionEnum region, bool embark, ActiveTrait active)
    {
        if ((!Deterable || !active.Detered) && (DurationData.Charges <= 0 || active.DurationData.Charges != 0) && IsMove)
        {
            EntityCard entityCard = (EntityCard) active.GetTraitTarget();
            entityCard.ClearActed(14);
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
            EntityCard entityCard = (EntityCard) active.GetTraitTarget();
            entityCard.ClearActed(14);
            if (active.HasCharges())
            {
                active.ExpendCharge(GameState);
            }
        }
    }

    public override void ActivateAction(CardStack location, RegionEnum region, ActiveTrait active)
    {
        if ((!Deterable || !active.Detered) && (DurationData.Charges <= 0 || active.DurationData.Charges != 0) &&
            IsActivate)
        {
            EntityCard entityCard = (EntityCard) active.GetTraitTarget();
            entityCard.ClearActed(14);
            if (active.HasCharges())
            {
                active.ExpendCharge(GameState);
            }
        }
    }

    public override void CheckCardDeployed(Card deployed, Card source)
    {
        if (Targets.Scope != 0 && Targets.Scope != TraitTargetScope.UnitStack &&
            DurationData.Type == TraitDurationType.Permanent)
        {
            CheckAndApplyTrait(deployed, source, true, false);
        }
    }
}