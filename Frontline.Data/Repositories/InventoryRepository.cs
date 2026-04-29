using Frontline.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Frontline.Data.Repositories;

public interface IInventoryRepository : IRepository<ItemEntity>
{
    Task<ItemEntity?> GetItemAsync(int userId, int itemId);
    Task<List<ItemEntity>> GetItems(int userId, int maxItems = 0);
    Task<List<ItemEntity>> GetItems(int userId, List<int> itemIds);
    Task<int> GetItemCountAsync(int userId);
    Task AddItemAsync(int userId, ItemEntity item);
    Task AddItemsAsync(int userId, List<ItemEntity> items);
    Task<bool> HasItemsAsync(int userId, List<int> itemIds);
    Task<List<ItemEntity>> GetUserItemsAsync(int userId);
}

public class InventoryRepository : RepositoryBase<ItemEntity>, IInventoryRepository
{
    public InventoryRepository(AppDb db) : base(db)
    {
    }

    public async Task<ItemEntity?> GetItemAsync(int userId, int itemId)
    {
        return await Db.Items
            .Where(item => item.UserId == userId && item.ItemId == itemId)
            .Select(item => new ItemEntity
            {
                UserId = item.UserId,
                ItemId = item.ItemId,
                TemplateId = item.TemplateId,
                Xp = item.Xp,
                Rank = item.Rank,
                Casualty = item.Casualty,
                DropshipId = Db.Dropships
                    .Where(dropshipItem => dropshipItem.UserId == userId
                                         && dropshipItem.ItemId == item.ItemId)
                    .Select(dropshipItem => (int?) dropshipItem.DropshipId)
                    .FirstOrDefault() ?? -1,
                CurrentMission = Db.ActiveMissions
                    .Where(mission => mission.UserId == userId
                                      && (mission.RequiredCardItemId == item.ItemId
                                          || mission.BonusCard1ItemId == item.ItemId
                                          || mission.BonusCard2ItemId == item.ItemId))
                    .Select(mission => mission.MissionKey)
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<ItemEntity>> GetItems(int userId, int maxItems = 0)
    {
        if (maxItems <= 0)
        {
            return await Db.Items
                .Where(item => item.UserId == userId)
                .OrderBy(item => item.ItemId)
                .Select(item => new ItemEntity
                {
                    UserId = item.UserId,
                    ItemId = item.ItemId,
                    TemplateId = item.TemplateId,
                    Xp = item.Xp,
                    Rank = item.Rank,
                    Casualty = item.Casualty,
                    DropshipId = Db.Dropships
                        .Where(dropshipItem => dropshipItem.UserId == userId
                                             && dropshipItem.ItemId == item.ItemId)
                        .Select(dropshipItem => (int?) dropshipItem.DropshipId)
                        .FirstOrDefault() ?? -1,
                    CurrentMission = Db.ActiveMissions
                        .Where(mission => mission.UserId == userId
                                          && (mission.RequiredCardItemId == item.ItemId
                                              || mission.BonusCard1ItemId == item.ItemId
                                              || mission.BonusCard2ItemId == item.ItemId))
                        .Select(mission => mission.MissionKey)
                        .FirstOrDefault()
                })
                .ToListAsync();
        }

        return await Db.Items
            .Where(item => item.UserId == userId)
            .OrderBy(item => item.ItemId)
            .Take(maxItems)
            .Select(item => new ItemEntity
            {
                UserId = item.UserId,
                ItemId = item.ItemId,
                TemplateId = item.TemplateId,
                Xp = item.Xp,
                Rank = item.Rank,
                Casualty = item.Casualty,
                DropshipId = Db.Dropships
                    .Where(dropshipItem => dropshipItem.UserId == userId
                                         && dropshipItem.ItemId == item.ItemId)
                    .Select(dropshipItem => (int?) dropshipItem.DropshipId)
                    .FirstOrDefault() ?? -1,
                CurrentMission = Db.ActiveMissions
                    .Where(mission => mission.UserId == userId
                                      && (mission.RequiredCardItemId == item.ItemId
                                          || mission.BonusCard1ItemId == item.ItemId
                                          || mission.BonusCard2ItemId == item.ItemId))
                    .Select(mission => mission.MissionKey)
                    .FirstOrDefault()
            })
            .ToListAsync();
    }

    public async Task<List<ItemEntity>> GetItems(int userId, List<int> itemIds)
    {
        return await Db.Items
            .Where(item => item.UserId == userId && itemIds.Contains(item.ItemId))
            .ToListAsync();
    }

    public async Task<int> GetItemCountAsync(int userId)
    {
        return await Db.Items
            .Where(item => item.UserId == userId)
            .CountAsync();
    }

    public async Task AddItemAsync(int userId, ItemEntity item)
    {
        item.UserId = userId;
        item.ItemId = await GetNextItemIdForUser(userId);

        await AddAsync(item);
    }

    public async Task AddItemsAsync(int userId, List<ItemEntity> items)
    {
        var nextItemId = await GetNextItemIdForUser(userId);

        foreach (var item in items)
        {
            item.UserId = userId;
            item.ItemId = nextItemId;
            nextItemId++;
        }

        await AddRangeAsync(items);
    }

    public async Task<List<ItemEntity>> GetUserItemsAsync(int userId)
    {
        return await Db.Items
            .Where(item => item.UserId == userId)
            .ToListAsync();
    }

    public async Task<bool> HasItemsAsync(int userId, List<int> itemIds)
    {
        return await Db.Items
            .Where(item => item.UserId == userId && itemIds.Contains(item.ItemId))
            .CountAsync() == itemIds.Count;
    }

    private async Task<int> GetNextItemIdForUser(int userId)
    {
        var maxItemId = await Db.Items
            .Where(item => item.UserId == userId)
            .MaxAsync(item => (int?) item.ItemId) ?? 0;
        return maxItemId + 1;
    }
}