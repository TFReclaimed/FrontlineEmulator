namespace Frontline.Battle.Traits;

public class StatModifierMultiply : StatModifierPassive
{
    public TraitTargeting CountInfo { get; set; }

    public override sbyte GetAttackBonus(Card target, ActiveTrait active)
    {
        if (Deterable && active.Detered)
        {
            return 0;
        }

        if (TraitTargeting.DoesMatchType(TargetType, TargetTypeMod.NumMods, 0, target))
        {
            var num = CountInfo.CalculateCount(GameState, active);
            var result = (sbyte) (IsAttack * num);
            if (IsAttack != 0 && active.DataValue != 0)
            {
                result = (sbyte) (active.DataValue * num);
            }

            return result;
        }

        return 0;
    }

    public override sbyte GetBypassDefenseBonus(Card target, ActiveTrait active)
    {
        if (Deterable && active.Detered)
        {
            return 0;
        }

        if (TraitTargeting.DoesMatchType(TargetType, TargetTypeMod.NumMods, 0, target))
        {
            var num = CountInfo.CalculateCount(GameState, active);
            var result = (sbyte) (BypassDefense * num);
            if (BypassDefense != 0 && active.DataValue != 0)
            {
                result = (sbyte) (active.DataValue * num);
            }

            return result;
        }

        return 0;
    }

    public override sbyte GetDefenseBonus(ActiveTrait active)
    {
        if (Deterable && active.Detered)
        {
            return 0;
        }

        var num = CountInfo.CalculateCount(GameState, active);
        var result = (sbyte) (Defense * num);
        if (Defense != 0 && active.DataValue != 0)
        {
            result = (sbyte) (active.DataValue * num);
        }

        return result;
    }

    public override sbyte GetHealthBonus(ActiveTrait active)
    {
        if (Deterable && active.Detered)
        {
            return 0;
        }

        var num = CountInfo.CalculateCount(GameState, active);
        var result = (sbyte) (Health * num);
        if (Health != 0 && active.DataValue != 0)
        {
            result = (sbyte) (active.DataValue * num);
        }

        return result;
    }

    public override sbyte GetCommandMod(ActiveTrait active)
    {
        if (Deterable && active.Detered)
        {
            return 0;
        }

        var num = CountInfo.CalculateCount(GameState, active);
        var result = (sbyte) (Command * num);
        if (Command != 0 && active.DataValue != 0)
        {
            result = (sbyte) (active.DataValue * num);
        }

        return result;
    }
}