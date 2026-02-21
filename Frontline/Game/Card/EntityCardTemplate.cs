using Frontline.Battle;

namespace Frontline.Game.Card;

public class EntityCardTemplate : CardTemplate
{
    public sbyte Health { get; set; }

    public override Battle.Card GenerateCard(CCG game, Battle.Card? source = null)
    {
        EntityCard entityCard;
        if (source != null)
        {
            entityCard = new EntityCard(game, source);
        }
        else
        {
            entityCard = new EntityCard(game);
            entityCard.SetTemplate(this);
            entityCard.TemplateId = CardId;
            entityCard.Rank = (sbyte) MinimumRank;
        }

        entityCard.Init();
        return entityCard;
    }

    public override bool CanDeploy(RegionEnum target, sbyte cardOwner)
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

    public override bool CanMove(RegionEnum target, sbyte cardOwner)
    {
        if ((uint) target == (byte) (0 + (byte) cardOwner))
        {
            return true;
        }

        if (target == RegionEnum.Control)
        {
            return Type == CardType.Pilot || Type == CardType.Titan;
        }

        return false;
    }

    public override bool CanMove(CCG gameState, CardStack source, CardStack target, bool emptyAvailable, bool embark)
    {
        if (target.PrimaryCard == null)
        {
            return true;
        }

        if (emptyAvailable)
        {
            return true;
        }

        Console.WriteLine("EntityTemplate.CanMove false - Target CardStack not Empty " + target.PrimaryCard.InstanceId);
        return false;
    }

    public override bool IsAttackable(CardStack source)
    {
        return true;
    }
}