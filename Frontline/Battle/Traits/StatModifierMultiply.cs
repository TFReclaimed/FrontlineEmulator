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
            int num = CountInfo.CalculateCount(GameState, active);
            sbyte result = (sbyte) (IsAttack * num);
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
            int num = CountInfo.CalculateCount(GameState, active);
            sbyte result = (sbyte) (BypassDefense * num);
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

        int num = CountInfo.CalculateCount(GameState, active);
        sbyte result = (sbyte) (Defense * num);
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

        int num = CountInfo.CalculateCount(GameState, active);
        sbyte result = (sbyte) (Health * num);
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

        int num = CountInfo.CalculateCount(GameState, active);
        sbyte result = (sbyte) (Command * num);
        if (Command != 0 && active.DataValue != 0)
        {
            result = (sbyte) (active.DataValue * num);
        }

        return result;
    }
}