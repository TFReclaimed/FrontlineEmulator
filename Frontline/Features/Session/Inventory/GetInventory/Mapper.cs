using FastEndpoints;
using Frontline.Data.Entities;
using Frontline.Game;
using Frontline.Missions;

namespace Frontline.Features.Session.Inventory.GetInventory;

public class Mapper : Mapper<GetInventoryRequest, List<InventoryCard>, List<ItemEntity>>
{
    public override List<InventoryCard> FromEntity(List<ItemEntity> e)
    {
        return e.Select(item => new InventoryCard
        {
            Type = GetCardType(item.TemplateId),
            InstanceId = item.ItemId,
            TemplateId = item.TemplateId,
            GameData = GetCardData(item),
            Xp = item.Xp,
            Rank = item.Rank
        }).ToList();
    }

    private static string GetCardType(int templateId)
    {
        if (RulesetParser.Ruleset is null)
        {
            return "Card";
        }

        var template = RulesetParser.Ruleset.CardsRuleset.Cards.Values.FirstOrDefault(x => x.CardId == templateId);
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