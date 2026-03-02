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
        EntityActionType b = 0;
        var entityCard = (EntityCard) card;
        var flag = entityCard.IsCardTraitsDetered();
        if (!active.GetTraitInfo().Deterable)
        {
            flag = false;
        }

        if (active.HasCharges() || active.HasDuration())
        {
            base.Apply(card, source, active);
            if (!Deploy || flag)
            {
                return;
            }

            if (Deploy)
            {
                b |= EntityActionType.Deploy;
            }

            if (IsAttack)
            {
                b |= EntityActionType.Attack;
            }

            if (IsMove)
            {
                b |= EntityActionType.Move;
            }

            if (IsActivate)
            {
                b |= EntityActionType.Activate;
            }

            entityCard.ClearActed(b);
        }
        else if (!flag)
        {
            if (Deploy)
            {
                b |= EntityActionType.Deploy;
            }

            if (IsAttack)
            {
                b |= EntityActionType.Attack;
            }

            if (IsMove)
            {
                b |= EntityActionType.Move;
            }

            if (IsActivate)
            {
                b |= EntityActionType.Activate;
            }

            entityCard.ClearActed(b);
        }
    }

    public override void Move(CardStack location, Region region, bool embark, ActiveTrait active)
    {
        if ((Deterable && active.Detered) || (DurationData.Charges > 0 && active.DurationData.Charges == 0) ||
            !IsMove)
        {
            return;
        }

        var entityCard = (EntityCard) active.GetTraitTarget();
        entityCard.ClearActed(EntityActionType.AnyButDeployMask);
        if (active.HasCharges())
        {
            active.ExpendCharge();
        }
    }

    public override void Attack(Card target, ActiveTrait active)
    {
        if ((Deterable && active.Detered) || (DurationData.Charges > 0 && active.DurationData.Charges == 0) ||
            !IsAttack)
        {
            return;
        }

        var entityCard = (EntityCard) active.GetTraitTarget();
        entityCard.ClearActed(EntityActionType.AnyButDeployMask);
        if (active.HasCharges())
        {
            active.ExpendCharge();
        }
    }

    public override void ActivateAction(CardStack location, Region region, ActiveTrait active)
    {
        if ((Deterable && active.Detered) || (DurationData.Charges > 0 && active.DurationData.Charges == 0) ||
            !IsActivate)
        {
            return;
        }

        var entityCard = (EntityCard) active.GetTraitTarget();
        entityCard.ClearActed(EntityActionType.AnyButDeployMask);
        if (active.HasCharges())
        {
            active.ExpendCharge();
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