using Frontline.Battle;

namespace Frontline.Game.Card;

public class CommanderCardTemplate : CardTemplate
{
    public required List<int> SupportIds { get; set; }

    public override Battle.Card GenerateCard(CcgGameState game, Battle.Card? source = null)
    {
        CommanderCard commanderCard;
        if (source != null)
        {
            commanderCard = new CommanderCard(game, source);
        }
        else
        {
            commanderCard = new CommanderCard(game, this);
            commanderCard.Rank = (sbyte) MinimumRank;
        }

        commanderCard.Init();
        return commanderCard;
    }

    public override bool CanDeploy(Region target, sbyte cardOwner)
    {
        return false;
    }

    public override bool CanDeploy(CardStack target, bool emptyAvailable, bool embark)
    {
        return false;
    }

    public override bool IsAttackable(CardStack source)
    {
        return true;
    }
}