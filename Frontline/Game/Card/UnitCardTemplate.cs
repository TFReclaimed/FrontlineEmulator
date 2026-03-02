using System.Text.Json.Serialization;
using Frontline.Battle;

namespace Frontline.Game.Card;

public class UnitCardTemplate : EntityCardTemplate
{
    public UnitType UnitType { get; set; }
    public sbyte Attack { get; set; }
    public sbyte Defense { get; set; }

    private FusionUpgradeSequence? _upgradeSequence;

    [JsonConstructor]
    public UnitCardTemplate()
    {
    }

    public UnitCardTemplate(UnitCardTemplate template)
    {
        Traits = new List<int>(template.Traits);
        CardId = template.CardId;
        Rarity = template.Rarity;
        Type = template.Type;
        IsHard = template.IsHard;
        Cost = template.Cost;
        MinimumRank = template.MinimumRank;
        Health = template.Health;
        Attack = template.Attack;
        Defense = template.Defense;
        UnitType = template.UnitType;
    }

    public void SetUpgradeSequence(FusionUpgradeSequence upgradeSequence)
    {
        _upgradeSequence = upgradeSequence;
    }

    public override CardTemplate GetRankedTemplate(sbyte rank)
    {
        if (_upgradeSequence == null)
        {
            return this;
        }

        rank = (sbyte) CalculateMinimumRank((byte) rank);
        var unitTemplate = new UnitCardTemplate(this);

        foreach (var upgrade in _upgradeSequence.Upgrades.Values)
        {
            if (upgrade.Rank > rank)
            {
                break;
            }

            unitTemplate.Attack += (sbyte) upgrade.Attack;
            unitTemplate.Health += (sbyte) upgrade.Health;
            unitTemplate.Defense += (sbyte) upgrade.Armor;

            if (upgrade.TraitId != 0)
            {
                unitTemplate.Traits.Add(upgrade.TraitId);
            }

            unitTemplate.Cost += (sbyte) upgrade.Command;
            unitTemplate.MinimumRank = (byte) upgrade.Rank;
        }

        return unitTemplate;
    }

    public override Battle.Card GenerateCard(CcgGameState game, Battle.Card? source = null)
    {
        UnitCard unitCard;
        if (source != null)
        {
            unitCard = new UnitCard(game, (UnitCard) source);
        }
        else
        {
            unitCard = new UnitCard(game, this);
            unitCard.Rank = (sbyte) MinimumRank;
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
            var primaryCard = target.PrimaryCard!;
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

    public override bool CanMove(CcgGameState gameState, CardStack source, CardStack target, bool emptyAvailable, bool embark)
    {
        if (base.CanMove(gameState, source, target, emptyAvailable, embark))
        {
            return true;
        }

        if (embark)
        {
            var primaryCard = target.PrimaryCard!;
            var primaryCard2 = source.PrimaryCard!;
            if (Type == CardType.Titan && primaryCard.GetTemplate().Type == CardType.Pilot)
            {
                if (!primaryCard2.HasPilot())
                {
                    var traitActorRegion = gameState.GetTraitActorRegion(primaryCard.ActiveData.Owner, primaryCard.InstanceId);
                    var traitActorRegion2 = gameState.GetTraitActorRegion(primaryCard2.ActiveData.Owner, primaryCard2.InstanceId);

                    if (traitActorRegion != traitActorRegion2)
                    {
                        gameState.Logger.Debug("UnitTemplate.CanMove false - not in the same region");
                        return false;
                    }

                    return true;
                }

                gameState.Logger.Debug("UnitTemplate.CanMove false - titan already piloted");
            }
            else if (Type == CardType.Pilot && primaryCard.GetTemplate().Type == CardType.Titan)
            {
                if (!primaryCard.HasPilot())
                {
                    var traitActorRegion3 = gameState.GetTraitActorRegion(primaryCard.ActiveData.Owner, primaryCard.InstanceId);
                    var traitActorRegion4 = gameState.GetTraitActorRegion(primaryCard2.ActiveData.Owner, primaryCard2.InstanceId);

                    if (traitActorRegion3 != traitActorRegion4)
                    {
                        gameState.Logger.Debug("UnitTemplate.CanMove false - not in the same region");
                        return false;
                    }

                    return true;
                }

                gameState.Logger.Debug("UnitTemplate.CanMove false - titan already piloted");
            }

            gameState.Logger.Debug("UnitTemplate.CanMove false - invalid pilot titan combo");
        }

        return false;
    }

    public override bool CanAttack(CardStack source, CardStack target)
    {
        var template = target.PrimaryCard!.GetTemplate();
        return template.IsAttackable(source);
    }

    private byte CalculateMinimumRank(byte requestedRank)
    {
        if (MinimumRank > requestedRank)
        {
            requestedRank = MinimumRank;
        }

        return requestedRank;
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