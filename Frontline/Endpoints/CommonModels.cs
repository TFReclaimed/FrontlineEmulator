using System.Text.Json.Serialization;
using Frontline.Battle.Data;
using Frontline.Battle.Data.Card;
using Frontline.Data.Entities;
using Frontline.Missions;
using Frontline.Utils;

namespace Frontline.Endpoints;

public class CardDto
{
    [JsonPropertyName("$type")]
    public string Type { get; set; } = "Card";
    public int InstanceId { get; set; }
    public int TemplateId { get; set; }
    [JsonConverter(typeof(JsonStringConverter<CardData>))]
    public CardData? GameData { get; set; }
    public int Xp { get; set; }
    public sbyte Rank { get; set; }

    public static CardDto FromEntity(ItemEntity item)
    {
        return new CardDto
        {
            Type = GetCardType(item.TemplateId),
            InstanceId = item.ItemId,
            TemplateId = item.TemplateId,
            GameData = GetCardData(item),
            Xp = item.Xp,
            Rank = item.Rank
        };
    }

    private static string GetCardType(int templateId)
    {
        if (RulesetParser.Ruleset is null)
        {
            return "Card";
        }

        var template = RulesetParser.Ruleset.CardsRuleset.Cards.Values
            .FirstOrDefault(x => x.CardId == templateId);
        return template is ResourceCardTemplate ? "ResourceCard" : "Card";
    }

    private static CardData? GetCardData(ItemEntity item)
    {
        // From the few videos on YouTube I believe that this behavior is correct,
        // however without the original server code it's impossible to be sure.
        if (item.IsInDropship && item.DropshipId != 0 && item.DropshipId != 1)
        {
            return new CardData
            {
                Availability = new CardAvailability
                {
                    CardState = CardState.InDropship
                }
            };
        }

        if (item.CurrentMission is not null)
        {
            return new CardData
            {
                Availability = GetMissionCardAvailability(item)
            };
        }

        if (item.Casualty)
        {
            return new CardData
            {
                Availability = new CardAvailability
                {
                    CardState = CardState.Casualty
                }
            };
        }

        return null;
    }

    private static CardAvailability GetMissionCardAvailability(ItemEntity item)
    {
        var missionKey = MissionsParser.ParseMissionKey(item.CurrentMission!);

        return new CardAvailability
        {
            CardState = CardState.OnMission,
            Region = missionKey.Region,
            Faction = missionKey.Faction,
            MissionId = missionKey.MissionId
        };
    }
}

public class CardData
{
    [JsonConverter(typeof(JsonStringConverter<CardAvailability>))]
    public CardAvailability? Availability { get; set; }
}

public class CardAvailability
{
    [JsonPropertyName("PvECardState")]
    [JsonConverter(typeof(JsonStringEnumConverter<CardState>))]
    public CardState CardState { get; set; }
    [JsonPropertyName("pve_region")]
    public PveRegion Region { get; set; }
    [JsonPropertyName("pve_faction")]
    public CardFaction Faction { get; set; }
    [JsonPropertyName("pve_missionid")]
    public int MissionId { get; set; }
}

public enum CardState
{
    None,
    OnMission,
    InDropship,
    Casualty
}