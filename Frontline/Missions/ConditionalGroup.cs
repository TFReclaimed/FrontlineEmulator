using Frontline.Data.Entities;
using Frontline.Game.Card;

namespace Frontline.Missions;

public class ConditionalGroup
{
    private readonly SortedDictionary<int, ConditionalStatement> _statements = [];

    public void AddConditional(MissionConditional conditional)
    {
        var group = conditional.Group;
        if (!_statements.TryGetValue(group, out var statement))
        {
            statement = new ConditionalStatement();
            _statements.Add(group, statement);
        }

        statement.AddConditional(conditional);
    }

    public bool Resolve(ItemEntity itemEntity, CardTemplate template)
    {
        var conjunction = Conjunction.Invalid;
        var result = false;

        foreach (var statement in _statements.Values)
        {
            result = statement.Resolve(itemEntity, template, out conjunction);
            if (conjunction == Conjunction.And && !result)
            {
                return false;
            }

            if (conjunction == Conjunction.Or && result)
            {
                return true;
            }
        }

        if (conjunction == Conjunction.And)
        {
            return true;
        }

        if (conjunction == Conjunction.Or)
        {
            return false;
        }

        return result;
    }
}