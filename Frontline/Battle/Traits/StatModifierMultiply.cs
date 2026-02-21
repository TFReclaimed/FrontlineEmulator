namespace Frontline.Battle.Traits;

public class StatModifierMultiply : StatModifierPassive
{
    public TraitTargeting countInfo;

    public override sbyte GetAttackBonus(Card target, ActiveTrait active)
    {
        if (deterable && active.detered)
        {
            return 0;
        }

        if (TraitTargeting.DoesMatchType(targetType, TargetTypeMod.NumMods, 0, target))
        {
            int num = countInfo.CalculateCount(GameState, active);
            sbyte result = (sbyte) (attack * num);
            if (attack != 0 && active.dataValue != 0)
            {
                result = (sbyte) (active.dataValue * num);
            }

            return result;
        }

        return 0;
    }

    public override sbyte GetBypassDefenseBonus(Card target, ActiveTrait active)
    {
        if (deterable && active.detered)
        {
            return 0;
        }

        if (TraitTargeting.DoesMatchType(targetType, TargetTypeMod.NumMods, 0, target))
        {
            int num = countInfo.CalculateCount(GameState, active);
            sbyte result = (sbyte) (bypassDefense * num);
            if (bypassDefense != 0 && active.dataValue != 0)
            {
                result = (sbyte) (active.dataValue * num);
            }

            return result;
        }

        return 0;
    }

    public override sbyte GetDefenseBonus(ActiveTrait active)
    {
        if (deterable && active.detered)
        {
            return 0;
        }

        int num = countInfo.CalculateCount(GameState, active);
        sbyte result = (sbyte) (defense * num);
        if (defense != 0 && active.dataValue != 0)
        {
            result = (sbyte) (active.dataValue * num);
        }

        return result;
    }

    public override sbyte GetHealthBonus(ActiveTrait active)
    {
        if (deterable && active.detered)
        {
            return 0;
        }

        int num = countInfo.CalculateCount(GameState, active);
        sbyte result = (sbyte) (health * num);
        if (health != 0 && active.dataValue != 0)
        {
            result = (sbyte) (active.dataValue * num);
        }

        return result;
    }

    public override sbyte GetCommandMod(ActiveTrait active)
    {
        if (deterable && active.detered)
        {
            return 0;
        }

        int num = countInfo.CalculateCount(GameState, active);
        sbyte result = (sbyte) (command * num);
        if (command != 0 && active.dataValue != 0)
        {
            result = (sbyte) (active.dataValue * num);
        }

        return result;
    }
}