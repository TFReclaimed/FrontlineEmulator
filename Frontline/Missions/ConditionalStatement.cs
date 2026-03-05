using Frontline.Data.Entities;
using Frontline.Game.Card;

namespace Frontline.Missions;

public class ConditionalStatement
{
    private delegate bool ResolutionFunction(MissionConditional conditional, ItemEntity item, CardTemplate template);

    private static readonly Dictionary<ConditionalAttribute, ResolutionFunction> ResolutionFunctions = new()
    {
        {
            ConditionalAttribute.IsType,
            IsType
        },
        {
            ConditionalAttribute.IsUnitType,
            IsUnitType
        },
        {
            ConditionalAttribute.HasTrait,
            HasTrait
        },
        {
            ConditionalAttribute.HasFlag,
            HasFlag
        },
        {
            ConditionalAttribute.IsName,
            IsName
        },
        {
            ConditionalAttribute.Command,
            CompareCommand
        },
        {
            ConditionalAttribute.IsRarity,
            CompareRarity
        },
        {
            ConditionalAttribute.Rank,
            CompareRank
        }
    };

    private readonly SortedList<float, MissionConditional> _conditionals = [];

    public void AddConditional(MissionConditional conditional)
    {
        _conditionals.Add(conditional.GroupPriority, conditional);
    }

    public bool Resolve(ItemEntity itemEntity, CardTemplate template, out Conjunction conjunction)
    {
        conjunction = Conjunction.Invalid;

        var result = false;
        foreach (var conditional in _conditionals.Values)
        {
            conjunction = conditional.Conjunction == Conjunction.None ? conjunction : conditional.Conjunction;

            if (!ResolutionFunctions.TryGetValue(conditional.Attribute, out var resolutionFunction))
            {
                return false;
            }

            result = resolutionFunction(conditional, itemEntity, template);
            if (conditional.Conjunction == Conjunction.And)
            {
                if (!result)
                {
                    conjunction = GetFinalConjunction();
                    return false;
                }
            }
            else if (conditional.Conjunction == Conjunction.Or && result)
            {
                conjunction = GetFinalConjunction();
                return true;
            }
        }

        conjunction = GetFinalConjunction();
        return result;
    }

    private Conjunction GetFinalConjunction()
    {
        if (_conditionals.Count == 0)
        {
            return Conjunction.Invalid;
        }

        return _conditionals.Values.Last().Conjunction;
    }

    private static bool IsType(MissionConditional conditional, ItemEntity item, CardTemplate template)
    {
        if (Enum.TryParse<CardType>(conditional.Comparison, out var type))
        {
            return CompareValues(conditional.Operator, template.Type, type);
        }

        return false;
    }

    private static bool IsUnitType(MissionConditional conditional, ItemEntity item, CardTemplate template)
    {
        if (Enum.TryParse<UnitType>(conditional.Comparison, out var unitType))
        {
            if (template is UnitCardTemplate unitCardTemplate)
            {
                return CompareValues(conditional.Operator, unitCardTemplate.UnitType, unitType);
            }
        }

        return false;
    }

    private static bool HasTrait(MissionConditional conditional, ItemEntity item, CardTemplate template)
    {
        if (int.TryParse(conditional.Comparison, out var traitId))
        {
            return template.Traits.Contains(traitId);
        }

        return false;
    }

    private static bool HasFlag(MissionConditional conditional, ItemEntity item, CardTemplate template)
    {
        switch (conditional.Comparison)
        {
            case "Hard":
                return CompareValues(conditional.Operator, template.IsHard, true);

            case "Soft":
                if (template.Type is not (CardType.Pilot or CardType.Support))
                {
                    return false;
                }

                return CompareValues(conditional.Operator, template.IsHard, false);

            case "Casualty":
                return item.Casualty;
        }

        return false;
    }

    private static bool IsName(MissionConditional conditional, ItemEntity item, CardTemplate template)
    {
        var nameMapping = MissionsParser.GetMissionNameMapping(conditional.Comparison);
        if (nameMapping is not null)
        {
            return CompareValues(conditional.Operator, item.TemplateId, nameMapping.TemplateId);
        }

        return false;
    }

    private static bool CompareCommand(MissionConditional conditional, ItemEntity item, CardTemplate template)
    {
        if (float.TryParse(conditional.Comparison, out var cost))
        {
            return CompareValues(conditional.Operator, template.Cost, cost);
        }

        return false;
    }

    private static bool CompareRarity(MissionConditional conditional, ItemEntity item, CardTemplate template)
    {
        if (Enum.TryParse<CardRarity>(conditional.Comparison, out var rarity))
        {
            return CompareValues(conditional.Operator, template.Rarity, rarity);
        }

        return false;
    }

    private static bool CompareRank(MissionConditional conditional, ItemEntity item, CardTemplate template)
    {
        if (int.TryParse(conditional.Comparison, out var rank))
        {
            return CompareValues(conditional.Operator, item.Rank, rank);
        }

        return false;
    }

    private static bool CompareValues(Operator @operator, object obj1, object obj2)
    {
        switch (@operator)
        {
            case Operator.IsEqual:
                return obj1.Equals(obj2);

            case Operator.IsNotEqual:
                return !obj1.Equals(obj2);

            case Operator.IsGreaterThan:
                return Convert.ToDouble(obj1) > Convert.ToDouble(obj2);

            case Operator.IsLessThan:
                return Convert.ToDouble(obj1) < Convert.ToDouble(obj2);

            case Operator.IsGreaterThanOrEqual:
                return Convert.ToDouble(obj1) >= Convert.ToDouble(obj2);

            case Operator.IsLessThanOrEqual:
                return Convert.ToDouble(obj1) <= Convert.ToDouble(obj2);

            default:
                return false;
        }
    }
}