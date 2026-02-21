using System.Text.Json.Serialization;
using Frontline.Battle;

namespace Frontline.Game.Card;

public class UnitCardTemplate : EntityCardTemplate
{
    public UnitType UnitType { get; set; }
    public sbyte Attack { get; set; }
    public sbyte Defense { get; set; }

    public override CardTemplate GetRankedTemplate(sbyte rank)
    {
        // TODO
        return base.GetRankedTemplate(rank);
    }

    public override Battle.Card GenerateCard(CCG game, Battle.Card? source = null)
    {
        UnitCard unitCard;
        if (source != null)
        {
            unitCard = new UnitCard(game, source);
        }
        else
        {
            unitCard = new UnitCard(game);
            unitCard.SetTemplate(this);
            unitCard.templateId = CardId;
            unitCard.rank = (sbyte) MinimumRank;
        }

        unitCard.Init();
        return unitCard;
    }

    public override bool CanDeploy(CardStack target, bool emptyAvailable, bool embark)
    {
        if (base.CanDeploy(target, emptyAvailable, embark))
        {
            return true;
        }

        if (embark)
        {
            Battle.Card primaryCard = target.primaryCard;
            if (Type == CardType.Titan && primaryCard.GetTemplate().Type == CardType.Pilot)
            {
                return true;
            }

            if (Type == CardType.Pilot && primaryCard.GetTemplate().Type == CardType.Titan && !primaryCard.HasPilot())
            {
                return true;
            }
        }

        return false;
    }

    public override bool CanMove(CCG gameState, CardStack source, CardStack target, bool emptyAvailable, bool embark)
    {
        if (base.CanMove(gameState, source, target, emptyAvailable, embark))
        {
            return true;
        }

        if (embark)
        {
            Battle.Card primaryCard = target.primaryCard;
            Battle.Card primaryCard2 = source.primaryCard;
            if (Type == CardType.Titan && primaryCard.GetTemplate().Type == CardType.Pilot)
            {
                if (!source.primaryCard.HasPilot())
                {
                    RegionEnum traitActorRegion = gameState.GetTraitActorRegion(primaryCard.activeData.owner, primaryCard.instanceId);
                    RegionEnum traitActorRegion2 = gameState.GetTraitActorRegion(primaryCard2.activeData.owner, primaryCard2.instanceId);

                    if (traitActorRegion != traitActorRegion2)
                    {
                        Console.WriteLine("UnitTemplate.CanMove false - not in the same region");
                        return false;
                    }

                    return true;
                }

                Console.WriteLine("UnitTemplate.CanMove false - titan already piloted");
            }
            else if (Type == CardType.Pilot && primaryCard.GetTemplate().Type == CardType.Titan)
            {
                if (!target.primaryCard.HasPilot())
                {
                    RegionEnum traitActorRegion3 = gameState.GetTraitActorRegion(primaryCard.activeData.owner, primaryCard.instanceId);
                    RegionEnum traitActorRegion4 = gameState.GetTraitActorRegion(primaryCard2.activeData.owner, primaryCard2.instanceId);

                    if (traitActorRegion3 != traitActorRegion4)
                    {
                        Console.WriteLine("UnitTemplate.CanMove false - not in the same region");
                        return false;
                    }

                    return true;
                }

                Console.WriteLine("UnitTemplate.CanMove false - titan already piloted");
            }

            Console.WriteLine("UnitTemplate.CanMove false - invalid pilot titan combo");
        }

        return false;
    }

    public override bool CanAttack(CardStack source, CardStack target)
    {
        CardTemplate template = target.primaryCard.GetTemplate();
        return template.IsAttackable(source);
    }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UnitType
{
    None,
    Light,
    Medium,
    Heavy,
    Stryder,
    Atlas,
    Ogre,
    Installation,
    Commander,
    Spectre,
    NumTypes
}