using Frontline.Battle;

namespace Frontline.Game.Card;

public class EntityCardTemplate : CardTemplate
{
    public sbyte Health { get; set; }

    public override Battle.Card GenerateCard(CcgGameState game, Battle.Card? source = null)
    {
        EntityCard entityCard;
        if (source != null)
        {
            entityCard = new EntityCard(game, source);
        }
        else
        {
            entityCard = new EntityCard(game, this);
            entityCard.Rank = (sbyte) MinimumRank;
        }

        entityCard.Init();
        return entityCard;
    }

    public override bool CanDeploy(Region target, sbyte cardOwner)
    {
        return (uint) target == (byte) (0 + (byte) cardOwner);
    }

    public override bool CanDeploy(CardStack target, bool emptyAvailable, bool embark)
    {
        if (target.PrimaryCard == null)
        {
            return true;
        }

        if (emptyAvailable && !embark)
        {
            return true;
        }

        return false;
    }

    public override bool CanMove(Region target, sbyte cardOwner)
    {
        if ((uint) target == (byte) (0 + (byte) cardOwner))
        {
            return true;
        }

        if (target == Region.Control)
        {
            return Type is CardType.Pilot or CardType.Titan;
        }

        return false;
    }

    public override bool CanMove(CcgGameState gameState, CardStack source, CardStack target, bool emptyAvailable, bool embark)
    {
        if (target.PrimaryCard == null)
        {
            return true;
        }

        if (emptyAvailable)
        {
            return true;
        }

        gameState.Logger.Debug("EntityTemplate.CanMove false - Target CardStack not empty {CardId}",
            target.PrimaryCard.InstanceId);
        return false;
    }

    public override bool IsAttackable(CardStack source)
    {
        return true;
    }
}