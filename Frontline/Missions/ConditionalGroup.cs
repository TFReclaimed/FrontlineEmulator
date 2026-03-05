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
        var result = false;

        foreach (var statement in _statements.Values)
        {
            result = statement.Resolve(itemEntity, template, out var conjunction);

            if (conjunction == Conjunction.And && !result)
            {
                return false;
            }

            if (conjunction == Conjunction.Or && result)
            {
                return true;
            }
        }

        return result;
    }
}