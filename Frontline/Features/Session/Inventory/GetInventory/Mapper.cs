using FastEndpoints;
using Frontline.Data.Entities;
using Frontline.Missions;

namespace Frontline.Features.Session.Inventory.GetInventory;

public class Mapper : Mapper<GetInventoryRequest, List<InventoryCard>, List<ItemEntity>>
{
    public override List<InventoryCard> FromEntity(List<ItemEntity> e)
    {
        return e.Select(item => new InventoryCard
        {
            InstanceId = item.ItemId,
            TemplateId = item.TemplateId,
            GameData = item.CurrentMission is null ? null : new CardData
            {
                Availability = GetMissionCardAvailability(item)
            },
            Xp = item.Xp,
            Rank = item.Rank
        }).ToList();
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