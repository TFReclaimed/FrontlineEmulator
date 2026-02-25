using System.Text.Json.Serialization;
using Frontline.Battle;

namespace Frontline.Game.Card;

[JsonDerivedType(typeof(CardTemplate), "CardTemplate")]
[JsonDerivedType(typeof(EntityCardTemplate), "EntityTemplate")]
[JsonDerivedType(typeof(UnitCardTemplate), "UnitTemplate")]
[JsonDerivedType(typeof(CommanderCardTemplate), "CommanderTemplate")]
[JsonDerivedType(typeof(ResourceCardTemplate), "ResourceCardTemplate")]
public class CardTemplate
{
    public List<int> Traits { get; set; } = [];
    public int CardId { get; set; }
    public CardRarity Rarity { get; set; }
    public CardType Type { get; set; }
    public bool IsHard { get; set; }
    public sbyte Cost { get; set; }
    [JsonPropertyName("rank")]
    public byte MinimumRank { get; set; }

    public virtual CardTemplate GetRankedTemplate(sbyte rank)
    {
        return this;
    }

    public virtual Battle.Card GenerateCard(CCG game, Battle.Card? source = null)
    {
        Battle.Card card;
        if (source != null)
        {
            card = new Battle.Card(game, source);
        }
        else
        {
            card = new Battle.Card(game);
            card.SetTemplate(this);
            card.TemplateId = CardId;
            card.Rank = (sbyte) MinimumRank;
        }

        card.Init();
        return card;
    }

    public int CreditsIfRetired(int xp)
    {
        var credits = Type switch
        {
            CardType.Pilot or CardType.Titan => 25 + (int) Math.Floor(xp * 0.2f),
            CardType.Support or CardType.BurnCard or CardType.Secret => 25,
            CardType.Commander => 300,
            _ => 0
        };

        credits = Rarity switch
        {
            CardRarity.Rare => (int) Math.Floor(credits * 1.25f),
            CardRarity.UltraRare => (int) Math.Floor(credits * 1.5f),
            CardRarity.Exclusive => (int) Math.Floor(credits * 2f),
            _ => credits
        };

        return credits;
    }

    public int XpIfRetired(int xp)
    {
        if (!IsCombatUnit())
        {
            return 0;
        }

        var receivedXp = 50 + (int) Math.Floor(xp * 0.5f);
        receivedXp = Rarity switch
        {
            CardRarity.Rare => (int) Math.Floor(receivedXp * 1.25f),
            CardRarity.UltraRare => (int) Math.Floor(receivedXp * 1.5f),
            CardRarity.Exclusive => (int) Math.Floor(receivedXp * 2f),
            _ => receivedXp
        };

        return receivedXp;
    }

    public virtual bool CanDeploy(Region target, sbyte cardOwner)
    {
        return true;
    }

    public virtual bool CanDeploy(CardStack target, bool emptyAvailable, bool embark)
    {
        return true;
    }

    public virtual bool CanMove(Region target, sbyte cardOwner)
    {
        return false;
    }

    public virtual bool CanMove(CCG gameState, CardStack source, CardStack target, bool emptyAvailable, bool embark)
    {
        return false;
    }

    public virtual bool CanDisembark(CardStack source)
    {
        return Type == CardType.Pilot;
    }

    public virtual bool CanAttack(CardStack source, CardStack target)
    {
        return false;
    }

    public virtual bool IsAttackable(CardStack source)
    {
        return false;
    }

    public bool IsCombatUnit()
    {
        return Type is CardType.Pilot or CardType.Titan;
    }

    public bool IsSupportUnit()
    {
        return Type == CardType.Support;
    }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CardRarity
{
    Common,
    Uncommon,
    Rare,
    UltraRare,
    Exclusive,
    NumRarities
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CardType
{
    Pilot,
    Titan,
    Support,
    BurnCard,
    Secret,
    Commander,
    Resource,
    NumTypes
}